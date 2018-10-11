using System.ComponentModel.DataAnnotations;

namespace TfsStates.Models
{
    public class TfsConnectionModel
    {
        [Required]
        public string Url { get; set; }

        public bool UseWindowsIdentity { get; set; }

        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public TfsConnectionValidationResult ValidationResult { get; set; }

        public bool IsSet()
        {
            return !string.IsNullOrEmpty(Url)
                && (UseWindowsIdentity || (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)));
        }
    }
}
