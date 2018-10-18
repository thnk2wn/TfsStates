using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;
using TfsStates.Models;
using TfsStates.Services;

namespace TfsStates.Controllers
{
    public class SettingsController : Controller
    {
        private const string ViewName = "~/Views/Home/Settings.cshtml";
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
                "https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=vsts");
            var model = await this.settingsService.GetSettingsOrDefault();
            return View(ViewName, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveConnection(
            [FromForm]
            TfsConnectionModel model
        )
        {
            model.ConnectionTypes = TfsConnectionTypes.Items;

            if (model.ConnectionType == TfsConnectionTypes.AzureDevOps 
                && string.IsNullOrEmpty(model.PersonalAccessToken))
            {
                ModelState.AddModelError(
                    nameof(model.PersonalAccessToken), 
                    "Personal Access Token is required");
            }

            if (!ModelState.IsValid)
            {
                return View(ViewName, model);
            }

            model = await this.settingsService.Save(model, validate: true);

            if (model.ValidationResult.IsError) { 
                ModelState.AddModelError(string.Empty, model.ValidationResult.Message);
            }
            else
            {
                await this.reportHistoryService.Clear();
            }

            return View(ViewName, model);
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