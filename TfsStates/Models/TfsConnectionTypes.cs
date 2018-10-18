using System.Collections.Generic;

namespace TfsStates.Models
{
    public static class TfsConnectionTypes
    {
        public const string TeamFoundationServer = "TFS Server / NTLM";

        public const string AzureDevOps = "Azure DevOps";

        public const string Default = TeamFoundationServer;

        public static List<string> Items = new List<string>
        {
            { TfsConnectionTypes.TeamFoundationServer },
            { TfsConnectionTypes.AzureDevOps }
        };
    }
}
