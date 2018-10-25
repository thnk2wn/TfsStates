using System.Linq;
using TfsStates.Models;
using TfsStates.ViewModels;

namespace TfsStates.Extensions
{
    public static class TfsKnownConnectionsExtensions
    {
        public static TfsConnectionViewModel ToViewModel(this TfsKnownConnections knownConnections)
        {
            var vm = new TfsConnectionViewModel
            {
                ActiveConnectionId = knownConnections.ActiveConnectionId,

                Connections = knownConnections
                    .Connections?
                    .Select(c => c.ToViewModel())
                    .ToList()
            };

            return vm;
        }
    }
}
