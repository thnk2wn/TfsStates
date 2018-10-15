using System.Linq;
using ElectronNET.API;
using TfsStates.Models;

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
}
