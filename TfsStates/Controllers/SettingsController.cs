using System.Threading.Tasks;
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
    }
}