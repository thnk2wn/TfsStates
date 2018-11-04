using System;
using Microsoft.VisualStudio.Services.WebApi;
using TfsStates.Models;
using TfsStates.Services;
using TfsStates.ViewModels;

namespace TfsStates.Extensions
{
    public static class TfsKnownConnectionExtensions
    {
        public static TfsConnectionItemViewModel ToViewModel(this TfsKnownConnection knownConn)
        {
            var vm = new TfsConnectionItemViewModel
            {
                ConnectionType = knownConn.ConnectionType,
                ConnectionTypes = TfsConnectionTypes.Items,
                Id = knownConn.Id,
                Name = knownConn.Name,
                Password = knownConn.Password,
                PersonalAccessToken = knownConn.PersonalAccessToken,
                Url = knownConn.Url,
                UseDefaultCredentials = knownConn.UseDefaultCredentials,
                Username = knownConn.Username
            };

            return vm;
        }

        public static VssConnection ToVssConnection(this TfsKnownConnection knownConn)
        {
            if (knownConn == null) return null;

            var creds = TfsCredentialsFactory.Create(knownConn);
            var vssConnection = new VssConnection(new Uri(knownConn.Url), creds);
            vssConnection.Settings.SendTimeout = TimeSpan.FromSeconds(AppSettings.DefaultTimeoutSeconds);

            return vssConnection;
        }
    }
}
