using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.WebApi;
using TfsStates.Services;

namespace TfsStates.Extensions
{
    public static class ITfsSettingsServiceExtensions
    {
        public static async Task<VssConnection> GetActiveVssConnection(this ITfsSettingsService service)
        {
            var activeConnection = await service.GetActiveConnection();
            if (activeConnection == null) return null;

            var creds = TfsCredentialsFactory.Create(activeConnection);
            var connection = new VssConnection(new Uri(activeConnection.Url), creds);
            return connection;
        }
    }
}
