using System;
using System.IO;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using TfsStates.Models;

namespace TfsStates.Controllers
{
    public class SettingsController : Controller
    {
        private const string ViewName = "~/Views/Settings/Index.cshtml";

        public async Task<IActionResult> Index()
        {
            var model = new TfsConnectionModel
            {
                UseWindowsIdentity = true
            };

            var filename = await GetSettingsFilename();

            if (System.IO.File.Exists(filename))
            {
                var json = await System.IO.File.ReadAllTextAsync(filename);
                model = JsonConvert.DeserializeObject<TfsConnectionModel>(json);
            }

            return View(model);
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

            string filename = await GetSettingsFilename();
            var json = JsonConvert.SerializeObject(model);
            await System.IO.File.WriteAllTextAsync(filename, json);

            model.Message = "Settings saved";

            return View(ViewName, model);
        }

        private async Task<string> GetSettingsFilename()
        {
            var appDataPath = await Electron.App.GetPathAsync(PathName.appData);
            var path = Path.Combine(appDataPath, this.GetType().Assembly.GetName().Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filename = Path.Combine(path, "settings.json");
            return filename;
        }

        public async Task<IActionResult> TestConnection(TfsConnectionModel model)
        {
            var client = new WorkItemTrackingHttpClient(
                new Uri(model.Url),
                new VssCredentials(true));

            const string teamProjectName = "AT_TEN";

            var rootIterationNode = await client.GetClassificationNodeAsync(
                teamProjectName,
                TreeStructureGroup.Iterations,
                depth: int.MaxValue);

            return Content("Hi");
        }
    }
}