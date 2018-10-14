using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;
using TfsStates.Models;
using TfsStates.Services;

namespace TfsStates.Controllers
{
    public class HomeController : Controller
    {
        private const string ViewName = "~/Views/Home/Index.cshtml";

        private readonly ITfsSettingsService settingsService;
        private readonly ITfsProjectService projectService;
        private readonly ITfsQueryService tfsQueryService;
        private readonly IExcelWriterService excelWriterService;
        private readonly IBroadcastService broadcastService;

        public HomeController(
            ITfsSettingsService settingsService,
            ITfsProjectService projectService,
            ITfsQueryService tfsQueryService, 
            IExcelWriterService excelWriterService,
            IBroadcastService broadcastService)
        {
            this.settingsService = settingsService;
            this.projectService = projectService;
            this.tfsQueryService = tfsQueryService;
            this.excelWriterService = excelWriterService;
            this.broadcastService = broadcastService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new TfsStatesModel();
            model.Projects.Insert(0, "- Select project -");
            var settingsUrl = Url.Action("Index", "Settings");

            try
            {
                var settings = await settingsService.GetSettings();

                if (settings == null)
                {
                    model.RunReadyState.NotReady(
                        $"<a href='{settingsUrl}'>TFS settings</a> need to first be supplied.");
                }
                else
                {
                    if (!settings.IsSet())
                    {
                        model.RunReadyState.NotReady(
                            $"Some <a href='{settingsUrl}'>TFS settings</a> are missing and " +
                            $"must first be supplied.");
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

            if (model.RunReadyState.State == TfsStatesModel.RunStates.NotReady) return View(model);

            try
            {
                var projectNames = await this.projectService.GetProjectNames();

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

            if (string.IsNullOrEmpty(model.RunReadyState.Message))
            {
                model.RunReadyState.IsReady = true;
            }            

            return View(model);
        }

        [Route("/home/iterations/{project}")]
        public async Task<IActionResult> GetIterations(string project)
        {
            var iterations = new List<string>();
            var projectIterations = await this.projectService.GetIterations(project);

            if (projectIterations != null)
            {
                iterations.AddRange(projectIterations);
            }

            return Json(iterations);
        }

        public async Task<IActionResult> RunReport(TfsStatesModel model)
        {
            await FileUtility.Cleanup();

            SendProgress($"Querying project {model.Project}, iteration under {model.Iteration}...");
            model.Results = await this.tfsQueryService.Query(model);

            var filename = await FileUtility.GetFilename($"TfsStates_{DateTime.Now.Ticks}.xlsx");
            SendProgress($"Writing {filename}...");

            var settings = await this.settingsService.GetSettings();
            var projectUrl = $"{settings.Url}/{model.Project}";
            this.excelWriterService.Write(filename, model.Results, projectUrl);

            SendProgress($"Launching {filename}...");
            System.Threading.Thread.Sleep(1000);

            model.ResultFilename = "file://" + filename;
            await Electron.Shell.OpenExternalAsync(model.ResultFilename);

            return View(ViewName, model);
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
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
