using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TfsStates.Models;
using TfsStates.Services;
using TfsStates.ViewModels;

namespace TfsStates.Controllers
{
    public class HomeController : Controller
    {
        private const string ViewName = "~/Views/Home/Index.cshtml";
        private const string NoProjectSelected = "- Select project -";
        private const string NoIterationSelected = "- Select iteration -";

        private readonly ITfsSettingsService settingsService;
        private readonly ITfsProjectService projectService;
        private readonly ITfsQueryService tfsQueryService;
        private readonly IExcelWriterService excelWriterService;
        private readonly IBroadcastService broadcastService;
        private readonly IReportHistoryService reportHistoryService;
        private readonly IChartService chartService;

        public HomeController(
            ITfsSettingsService settingsService,
            ITfsProjectService projectService,
            ITfsQueryService tfsQueryService, 
            IExcelWriterService excelWriterService,
            IBroadcastService broadcastService,
            IReportHistoryService reportHistoryService,
            IChartService chartService)
        {
            this.settingsService = settingsService;
            this.projectService = projectService;
            this.tfsQueryService = tfsQueryService;
            this.excelWriterService = excelWriterService;
            this.broadcastService = broadcastService;
            this.reportHistoryService = reportHistoryService;
            this.chartService = chartService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new TfsStatesViewModel();
            await LoadLookups(model);

            if (string.IsNullOrEmpty(model.RunReadyState.Message))
            {
                model.RunReadyState.IsReady = true;
            }

            return View(model);
        }

        [Route("/home/connections/{connectionId}/projects")]
        public async Task<IActionResult> GetProjects(Guid connectionId)
        {
            var connection = await this.settingsService.GetConnection(connectionId);

            var items = new List<string> { { NoProjectSelected } };
            var projectNames = await this.projectService.GetProjectNames(connection);

            if (projectNames != null)
            {
                items.AddRange(projectNames);
            }

            return Json(items);
        }

        [Route("/home/iterations/{connectionId}/{project}")]
        public async Task<IActionResult> GetIterations(Guid connectionId, string project)
        {
            var iterations = await GetIterationData(connectionId, project);
            return Json(iterations);
        }

        private async Task<List<string>> GetIterationData(Guid connectionId, string project)
        {
            var knownConn = await this.settingsService.GetConnection(connectionId);
            var iterations = await GetIterationData(knownConn, project);
            return iterations;
        }

        private async Task<List<string>> GetIterationData(TfsKnownConnection knownConn, string project)
        {
            var iterations = new List<string>
            {
                {NoIterationSelected}
            };

            var projectIterations = await this.projectService.GetIterations(knownConn, project);

            if (projectIterations != null)
            {
                iterations.AddRange(projectIterations);
            }

            return iterations;
        }

        public async Task<IActionResult> RunReport(TfsStatesViewModel viewModel)
        {
            if (viewModel.ConnectionId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(viewModel.ConnectionId), "Connection is required");
            }

            if (string.IsNullOrEmpty(viewModel.Project) || viewModel.Project == NoProjectSelected)
            {
                ModelState.AddModelError(nameof(viewModel.Project), "Project is required");
            }

            if (string.IsNullOrEmpty(viewModel.Iteration) || viewModel.Iteration == NoIterationSelected)
            {
                ModelState.AddModelError(
                    nameof(viewModel.Iteration), 
                    "Iteration Under is required (you can select a parent iteration container)");
            }

            await LoadLookups(viewModel);

            if (!ModelState.IsValid) 
            { 
                return View(ViewName, viewModel);
            }

            var sw = Stopwatch.StartNew();
            await FileUtility.Cleanup();

            TfsQueryResult queryResult = null;

            var knownConn = await this.settingsService.GetConnection(viewModel.ConnectionId);

            try 
            { 
                SendProgress($"Querying project {viewModel.Project}, iteration under {viewModel.Iteration}...");
                queryResult = await this.tfsQueryService.Query(knownConn, viewModel);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                var settingsUrl = Url.Action("Index", "Settings");
                viewModel.RunReadyState.NotReady(
                    $"Error querying TFS. Verify <a href='{settingsUrl}'>TFS settings</a> " +
                    $"and your connectivity and try again.");
                return View(ViewName, viewModel);
            }

            if (queryResult.TfsItems.Count > 0)
            {
                var fName = $"TfsStates_{DateTime.Now.Ticks}.xlsx";
                var filename = await FileUtility.GetFilename(fName);
                SendProgress($"Writing {filename}...");

                var projectUrl = $"{knownConn.Url}/{viewModel.Project}";
                this.excelWriterService.Write(filename, queryResult.TfsItems, projectUrl);

                SendProgress($"Launching {filename}...");
                System.Threading.Thread.Sleep(1000);

                viewModel.ResultFilename = fName;

                await Electron.Shell.OpenExternalAsync(filename);
            }

            // eat file in use exception
            try 
            { 
                await this.reportHistoryService.Record(knownConn.Id, viewModel.Project, viewModel.Iteration);
            }
            catch (IOException ioEx) { }

            if (queryResult.TfsItems.Count > 0)
            {
                var chart = chartService.CreateBarChart(queryResult);
                ViewData["chart"] = chart;
            }

            sw.Stop();
            var items = queryResult.TfsItems;
            var totalTransitions = items.Sum(x => x.TransitionCount);
            var avgTransitions = items.Count > 0 
                ? Math.Round(items.Average(x => x.TransitionCount), 0)
                : 0;

            viewModel.FinalProgress = new ReportProgress
            {
                WorkItemsProcessed = queryResult.TotalWorkItems,
                Message = $"Processed {"work item".ToQuantity(queryResult.TotalWorkItems, "###,##0")} and " +
                    $"{"revision".ToQuantity(queryResult.TotalRevisions, "###,##0")} in {sw.Elapsed.Humanize()}. " +
                    $"{"transition".ToQuantity(totalTransitions, "###,##0")}. " +
                    $"Average work item transitions: {avgTransitions}"
            };

            return View(ViewName, viewModel);
        }

        [Route("/home/viewreport/{name}")]
        public async Task<IActionResult> ViewReport(string name)
        {
            var filename = await FileUtility.GetFilename(name);
            await Electron.Shell.OpenExternalAsync(filename);
            return Json(filename);
        }

        private void RegisterOpenWebLink(string channel, string url)
        {
            if (HybridSupport.IsElectronActive) 
            { 
                Electron.IpcMain.On(channel, async (args) =>
                {
                    await Electron.Shell.OpenExternalAsync(url);
                });
            }
        }

        private bool ShouldSelectConnection(TfsKnownConnection knownConn, ReportRun lastReportRun)
        {
            if (lastReportRun == null) return false;
            var select = knownConn.Id == lastReportRun.ConnectionId;
            return select;
        }

        private async Task LoadLookups(TfsStatesViewModel model)
        {
            model = model ?? new TfsStatesViewModel();

            if (model.Projects?.Any() ?? false) return;

            model.Connections.Add(new SelectListItem
            {
                Text = "- Select connection -"
            });

            var settingsUrl = Url.Action("Index", "Settings");            
            model.Projects.Insert(0, NoProjectSelected);

            TfsKnownConnection defaultConnection = null;
            var lastReportRun = await this.reportHistoryService.GetLastRunSettings();

            try
            {
                var connections = await settingsService.GetConnections();

                if (connections == null || (connections.Connections?.Count ?? 0) == 0)
                {
                    model.RunReadyState.NotReady(
                        $"<a href='{settingsUrl}'>TFS settings</a> need to first be supplied.");
                }
                else
                {
                    model.Connections.AddRange(
                        connections
                        .Connections
                        .OrderBy(c => c.Name)
                        .Select(c =>
                            new SelectListItem
                            {
                                Text = c.Name,
                                Value = c.Id.ToString(),
                                Selected = (1 == connections.Connections.Count
                                    || ShouldSelectConnection(c, lastReportRun))
                            }));

                    if (model.ConnectionId == Guid.Empty 
                        && lastReportRun != null 
                        && connections.Connection(lastReportRun.ConnectionId) != null)
                    {
                        defaultConnection = connections.Connection(lastReportRun.ConnectionId);
                    }
                    else if (model.ConnectionId != Guid.Empty)
                    {
                        defaultConnection = connections.Connection(model.ConnectionId);
                    }
                    else if (connections.Connections.Count == 1)
                    {
                        defaultConnection = connections.Connections.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                model.RunReadyState.NotReady(
                        $"Error loading <a href='{settingsUrl}'>TFS settings</a>. " +
                        $"Adjust settings and try again.");
            }            

            if (model.RunReadyState.State == TfsStatesViewModel.RunStates.NotReady) return;

            if (defaultConnection != null)
            {
                model.ConnectionId = defaultConnection.Id;

                try
                {
                    var projectNames = await this.projectService.GetProjectNames(defaultConnection);

                    if (projectNames == null || !projectNames.Any())
                    {
                        model.RunReadyState.NotReady(
                            $"No TFS projects found. Check <a href='{settingsUrl}'>TFS settings</a> " +
                            $"and try again.");
                    }
                    else
                    {
                        model.Projects.AddRange(projectNames);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    model.RunReadyState.NotReady(
                        $"Error loading TFS projects. Check <a href='{settingsUrl}'>TFS settings</a> " +
                        $"and your connectivity and try again.");
                }

                if (model.RunReadyState.State == TfsStatesViewModel.RunStates.NotReady) return;
            }
            

            if (lastReportRun != null && defaultConnection != null)
            {
                if (model.Projects?.Any() ?? false 
                    && lastReportRun.ConnectionId == defaultConnection.Id
                    && model.Projects.Contains(lastReportRun.Project))
                {
                    model.Project = lastReportRun.Project;
                }
            }

            if (defaultConnection != null
                && !string.IsNullOrEmpty(model.Project)
                && model.Project != NoProjectSelected
                && (!model.Iterations?.Any() ?? false))
            {
                try 
                { 
                    model.Iterations = await GetIterationData(defaultConnection, model.Project);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    model.RunReadyState.NotReady(
                        $"Error loading TFS iterations. Check <a href='{settingsUrl}'>TFS settings</a> " +
                        $"and your connectivity and try again.");
                }
            }

            if (lastReportRun != null)
            {
                if (model.Iterations?.Any() ?? false && model.Iterations.Contains(lastReportRun.Iteration))
                {
                    model.Iteration = lastReportRun.Iteration;
                }
            }
        }

        private void SendProgress(string message, int? processedCount = null)
        {
            var progress = new ReportProgress 
            { 
                Message = message,
                WorkItemsProcessed = processedCount
            };

            this.broadcastService.ReportProgress(progress);
        }

        public IActionResult About()
        {
            RegisterOpenWebLink("link-about", "https://geoffhudik.com/about/");
            return View();
        }

        public IActionResult Contact()
        {
            RegisterOpenWebLink("link-contact", "https://geoffhudik.com/contact/");
            RegisterOpenWebLink("link-source", "https://github.com/thnk2wn/TfsStates");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }        
    }
}
