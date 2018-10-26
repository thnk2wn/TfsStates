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
                UseDefaultCredentials = true
            };

            return PartialView(ConnectionItemViewName, model);
        }

        public async Task<IActionResult> SaveConnection(TfsConnectionItemViewModel viewModel)
        {
            viewModel.ConnectionTypes = TfsConnectionTypes.Items;

            if (viewModel.ConnectionType == TfsConnectionTypes.AzureDevOpsToken
                && string.IsNullOrEmpty(viewModel.PersonalAccessToken))
            {
                ModelState.AddModelError(
                    nameof(viewModel.PersonalAccessToken),
                    "Personal Access Token is required");
            }

            if (!ModelState.IsValid)
            {
                return PartialView(ConnectionItemViewName, viewModel);
            }

            var knownConn = viewModel.ToKnownConnection();
            viewModel.ValidationResult = await this.settingsService.Validate(knownConn);

            if (!viewModel.ValidationResult.IsError) 
            {
                await this.settingsService.Save(knownConn);
                await this.reportHistoryService.Clear();
            }
            else
            {
                ModelState.AddModelError(string.Empty, viewModel.ValidationResult.Message);
            }

            if (!viewModel.ValidationResult.IsError)
            {
                await this.reportHistoryService.Clear();
            }

            return PartialView(ConnectionItemViewName, viewModel);
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