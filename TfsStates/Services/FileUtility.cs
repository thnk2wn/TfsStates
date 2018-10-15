using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace TfsStates.Services
{
    public static class FileUtility
    {
        public static async Task<string> GetFilename(string name)
        {
            var appDataPath = await Electron.App.GetPathAsync(PathName.appData);
            var path = Path.Combine(appDataPath, typeof(FileUtility).Assembly.GetName().Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filename = Path.Combine(path, name);
            return filename;
        }

        public static async Task Cleanup(TimeSpan? olderThan = null)
        {
            olderThan = olderThan ?? TimeSpan.FromDays(3);
            var appDataPath = await Electron.App.GetPathAsync(PathName.appData);
            var path = Path.Combine(appDataPath, typeof(FileUtility).Assembly.GetName().Name);
            var dir = new DirectoryInfo(path);

            if (!dir.Exists) return;

            var files = dir.GetFiles("*.xslx", SearchOption.TopDirectoryOnly)
                .Where(fi => fi.LastWriteTime.Add(olderThan.Value) < DateTime.Now);

            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }            
        }
    }
}
