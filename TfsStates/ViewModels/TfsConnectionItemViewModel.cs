using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TfsStates.Models;

namespace TfsStates.ViewModels
{
    public class TfsConnectionItemViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Url { get; set; }

        [DisplayName("Use Default Credentials")]
        public bool UseDefaultCredentials { get; set; }

        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public TfsConnectionValidationResult ValidationResult { get; set; }

        [Required]
        [DisplayName("Connection Type")]
        public string ConnectionType { get; set; }

        public List<string> ConnectionTypes { get; set; }

        [DisplayName("Personal Access Token")]
        [DataType(DataType.Password)]
        public string PersonalAccessToken { get; set; }

        public bool IsSet()
        {
            return !string.IsNullOrEmpty(Url)
                && (UseDefaultCredentials || (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)));
        }
    }
}
