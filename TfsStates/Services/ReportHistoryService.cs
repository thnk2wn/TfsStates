using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsStates.Models;

namespace TfsStates.Services
{
    public class ReportHistoryService : IReportHistoryService
    {
        public async Task Record(Guid knownConnId, string project, string iteration)
        {
            var filename = await GetFilename();

            var run = new ReportRun
            {
                ConnectionId = knownConnId,
                Project = project,
                Iteration = iteration,
            };

            var json = JsonConvert.SerializeObject(run);
            await File.WriteAllTextAsync(filename, json);
        }

        public async Task<ReportRun> GetLastRunSettings()
        {
            var filename = await GetFilename();

            if (!File.Exists(filename)) return null;

            var json = await File.ReadAllTextAsync(filename);
            var run = JsonConvert.DeserializeObject<ReportRun>(json);

            return run;
        }

        public async Task Clear()
        {
            var filename = await GetFilename();
            if (!File.Exists(filename)) return;
            File.Delete(filename);
        }

        private async Task<string> GetFilename()
        {
            var filename = await FileUtility.GetFilename("LastReportRun.json");
            return filename;
        }
    }
}
