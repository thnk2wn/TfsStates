using System.IO;
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
    }
}
