using TfsStates.Models;
using TfsStates.ViewModels;

namespace TfsStates.Extensions
{
    public static class TfsConnectionItemModelExtensions
    {
        public static TfsKnownConnection ToKnownConnection(this TfsConnectionItemViewModel viewModel)
        {
            // TODO: Consider AutoMapper if mapping grows much.

            var knownConn = new TfsKnownConnection
            {
                ConnectionType = viewModel.ConnectionType,
                Id = viewModel.Id,
                Password = viewModel.Password,
                PersonalAccessToken = viewModel.PersonalAccessToken,
                Url = viewModel.Url,
                UseDefaultCredentials = viewModel.UseDefaultCredentials,
                Username = viewModel.Username
            };

            return knownConn;
        }
    }
}
