using System.Collections.Generic;

namespace TfsStates.Models
{
    public static class TfsConnectionTypes
    {
        public const string TfsNTLM = "TFS Server - NTLM";

        public const string AzureDevOpsActiveDir = "Azure DevOps - Active Directory";

        public const string AzureDevOpsToken = "Azure DevOps - Token";

        public const string Default = TfsNTLM;

        public static List<string> Items = new List<string>
        {
            { AzureDevOpsActiveDir },
            { AzureDevOpsToken },
            { TfsNTLM }           
        };
    }
}
