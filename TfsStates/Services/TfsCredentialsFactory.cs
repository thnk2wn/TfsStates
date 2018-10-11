using System.Net;
using Microsoft.VisualStudio.Services.Common;
using TfsStates.Models;

namespace TfsStates.Services
{
    public static class TfsCredentialsFactory
    {
        public static VssCredentials Create(TfsConnectionModel model)
        {
            var creds = model.UseWindowsIdentity 
                ? new VssCredentials(true)
                : new VssCredentials(
                    new WindowsCredential(
                        new NetworkCredential(model.Username, model.Password)));
            return creds;
        }
    }
}
