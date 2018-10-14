using System.Linq;
using ElectronNET.API;

namespace TfsStates.Services
{
    public class BroadcastService : IBroadcastService
    {
        public void ReportProgress(ReportProgress progress)
        {
            SendProgress("report-progress", progress);
        }

        private void SendProgress(string channel, params object[] data)
        {
            var mainWindow = Electron.WindowManager.BrowserWindows.First();
            Electron.IpcMain.Send(mainWindow, channel, data);
        }
    }

    public class ReportProgress
    {
        public string Message { get; set; }

        public int? WorkItemsProcessed { get; set; }
    }
}
