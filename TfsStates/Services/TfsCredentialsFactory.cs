using System.Net;
using Microsoft.VisualStudio.Services.Common;
using TfsStates.Models;

namespace TfsStates.Services
{
    public static class TfsCredentialsFactory
    {
        // TODO: Add support for OAuth and/or personal access token for Azure DevOps with changes to settings view
        // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=vsts
        // https://stackoverflow.com/questions/46719764/cant-access-my-repos-in-vsts-using-rest-api
        public static VssCredentials Create(TfsConnectionModel model)
        {
            VssCredentials creds = null;

            if (model.ConnectionType == TfsConnectionTypes.TeamFoundationServer)
            {
                creds = model.UseWindowsIdentity
                    ? new VssCredentials(true)
                    : new VssCredentials(
                        new WindowsCredential(
                            new NetworkCredential(model.Username, model.Password)));
            }
            else if (model.ConnectionType == TfsConnectionTypes.AzureDevOps)
            {
                creds = new VssBasicCredential(string.Empty, model.PersonalAccessToken);
            }

            return creds;
        }
    }
}
