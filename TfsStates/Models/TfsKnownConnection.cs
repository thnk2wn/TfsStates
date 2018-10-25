using System;

namespace TfsStates.Models
{
    public class TfsKnownConnection
    {
        public Guid Id { get; set; }

        public string Url { get; set; }

        public bool UseDefaultCredentials { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ConnectionType { get; set; }
        
        public string PersonalAccessToken { get; set; }

        public DateTime? AddedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsSet()
        {
            if (string.IsNullOrEmpty(Url)) return false;

            if (ConnectionType == TfsConnectionTypes.TfsNTLM || ConnectionType == TfsConnectionTypes.AzureDevOpsActiveDir)
            { 
                return (UseDefaultCredentials || (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)));
            }
            else if (ConnectionType == TfsConnectionTypes.AzureDevOpsToken)
            {
                return !string.IsNullOrEmpty(PersonalAccessToken);
            }
            
            throw new NotSupportedException($"Unexpected connection type {ConnectionType}");
        }
    }
}
