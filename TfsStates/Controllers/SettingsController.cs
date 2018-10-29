using System;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;
using TfsStates.Extensions;
using TfsStates.Models;
using TfsStates.Services;
using TfsStates.ViewModels;

namespace TfsStates.Controllers
{
    public class SettingsController : Controller
    {
        private const string ViewName = "~/Views/Home/Settings.cshtml";
        private const string ConnectionItemViewName = "~/Views/Home/TfsConnectionItem.cshtml";
        private readonly ITfsSettingsService settingsService;
        private readonly IReportHistoryService reportHistoryService;

        public SettingsController(
            ITfsSettingsService settingsService,
            IReportHistoryService reportHistoryService)
        {
            this.settingsService = settingsService;
            this.reportHistoryService = reportHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            RegisterOpenWebLink(
                "azure-devops-pat-docs",
                "https://github.com/thnk2wn/TfsStates/wiki/Authenticating-with-Azure-DevOps");

            RegisterOpenWebLink(
                "azure-devops-pat-msdocs",
                "https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=vsts");

            try
            {
                var viewModel = (await this.settingsService.GetConnectionsOrDefault())
                    .ToViewModel();
                return View(ViewName, viewModel);
            }
            catch (Exception ex)
            {
                var filename = await this.settingsService.GetFilename();
                ModelState.AddModelError(
                    string.Empty,
                    $"Error loading settings. You may want to delete or manually edit {filename}.");
                return View(ViewName, new TfsConnectionViewModel());
            }
        }
        
        public IActionResult NewConnection()
        {
            var model = new TfsConnectionItemViewModel 
            {
                Id = Guid.NewGuid(),
                ConnectionTypes = TfsConnectionTypes.Items,
                ConnectionType = TfsConnectionTypes.AzureDevOpsToken,
                UseDefaultCredentials = false
            };

            return PartialView(ConnectionItemViewName, model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveConnection(TfsConnectionItemViewModel viewModel)
        {
            viewModel.ConnectionTypes = TfsConnectionTypes.Items;

            var knownConn = await ValidateConnection(viewModel);

            if (!ModelState.IsValid)
            {
                viewModel.TestMode = false;
                return PartialView(ConnectionItemViewName, viewModel);
            }

            if (!viewModel.TestMode)
            {
                await this.settingsService.Save(knownConn);
                await this.reportHistoryService.Clear();
                viewModel.ValidationResult.Message = $"Settings saved. {viewModel.ValidationResult.Message}";
            }

            viewModel.TestMode = false;
            return PartialView(ConnectionItemViewName, viewModel);
        }

        [HttpDelete("/settings/remove-connection/{connectionId}")]
        public async Task<IActionResult> RemoveConnection([FromRoute]Guid connectionId)
        {
            await this.settingsService.Remove(connectionId);
            return Json(true);
        }

        private async Task<TfsKnownConnection> ValidateConnection(TfsConnectionItemViewModel viewModel)
        {
            if (viewModel.ConnectionType == TfsConnectionTypes.AzureDevOpsToken)
            {
                if (string.IsNullOrEmpty(viewModel.PersonalAccessToken))
                {
                    ModelState.AddModelError(
                        nameof(viewModel.PersonalAccessToken),
                        "Personal Access Token is required");
                }

                if (viewModel.UseDefaultCredentials)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.UseDefaultCredentials),
                        "Use default credentials is not applicable with personal access tokens");
                }
            }
            else if (viewModel.ConnectionType == TfsConnectionTypes.TfsNTLM)
            {
                if (!string.IsNullOrEmpty(viewModel.PersonalAccessToken))
                {
                    ModelState.AddModelError(
                        nameof(viewModel.PersonalAccessToken),
                        "Personal Access Token is not applicable when using credentials");
                }

                if (viewModel.UseDefaultCredentials
                    && (!string.IsNullOrEmpty(viewModel.Username) || !string.IsNullOrEmpty(viewModel.Password)))
                {
                    ModelState.AddModelError(
                        nameof(viewModel.UseDefaultCredentials),
                        "Username and password are not applicable when using default credentials");
                }
            }

            if (!ModelState.IsValid) return null;

            var knownConn = viewModel.ToKnownConnection();
            viewModel.ValidationResult = await this.settingsService.Validate(knownConn);

            if (viewModel.ValidationResult.IsError)
            {
                ModelState.AddModelError(string.Empty, viewModel.ValidationResult.Message);
                return null;
            }

            return knownConn;
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
    }
}