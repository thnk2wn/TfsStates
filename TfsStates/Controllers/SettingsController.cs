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
            var viewModel = (await this.settingsService.GetConnectionsOrDefault())
                .ToViewModel();
            return View(ViewName, viewModel);
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

        public async Task<IActionResult> SaveConnection(TfsConnectionItemViewModel viewModel)
        {
            viewModel.ConnectionTypes = TfsConnectionTypes.Items;

            var knownConn = await ValidateConnection(viewModel);

            if (!ModelState.IsValid)
            {
                return PartialView(ConnectionItemViewName, viewModel);
            }

            await this.settingsService.Save(knownConn);
            await this.reportHistoryService.Clear();

            return PartialView(ConnectionItemViewName, viewModel);
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