using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TfsStates.Models
{
    public class TfsConnectionModel
    {
        [Required]
        public string Url { get; set; }

        [DisplayName("Use Windows Identity")]
        public bool UseWindowsIdentity { get; set; }

        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public TfsConnectionValidationResult ValidationResult { get; set; }

        [Required]
        [DisplayName("Connection Type")]
        public string ConnectionType { get; set; }

        public List<string> ConnectionTypes { get; set; }

        [DisplayName("Personal Access Token")]
        public string PersonalAccessToken { get; set; }

        public bool IsSet()
        {
            return !string.IsNullOrEmpty(Url)
                && (UseWindowsIdentity || (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)));
        }
    }
}
