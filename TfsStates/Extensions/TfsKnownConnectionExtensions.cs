using TfsStates.Models;
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
    }
}
