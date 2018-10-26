using System.Net;
using Microsoft.VisualStudio.Services.Common;
using TfsStates.Models;

namespace TfsStates.Services
{
    public static class TfsCredentialsFactory
    {
        // https://docs.microsoft.com/en-us/azure/devops/integrate/get-started/client-libraries/samples?view=vsts
        // https://blog.devmatter.com/personal-access-tokens-and-vsts-apis/
        // https://stackoverflow.com/questions/46719764/cant-access-my-repos-in-vsts-using-rest-api
        public static VssCredentials Create(TfsKnownConnection connection)
        {
            VssCredentials creds = null;

            if (connection.ConnectionType == TfsConnectionTypes.TfsNTLM)
            {
                creds = connection.UseDefaultCredentials
                    ? new VssCredentials(true)
                    : new VssCredentials(
                        new WindowsCredential(
                            new NetworkCredential(connection.Username, connection.Password)));
            }
            /*
            else if (connection.ConnectionType == TfsConnectionTypes.AzureDevOpsActiveDir)
            {
                if (connection.UseDefaultCredentials)
                    creds = new VssAadCredential();
                else 
                    creds = new VssAadCredential(connection.Username, connection.Password);
            }
            */
            else if (connection.ConnectionType == TfsConnectionTypes.AzureDevOpsToken)
            {
                creds = new VssBasicCredential(string.Empty, connection.PersonalAccessToken);
            }

            return creds;
        }
    }
}
