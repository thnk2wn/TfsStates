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
                Connections = knownConnections
                    .Connections?
                    .Select(c => c.ToViewModel())
                    .ToList()
            };

            return vm;
        }

        public static bool Any(this TfsKnownConnections connections)
        {
            return connections?.Connections != null && connections.Connections.Any();
        }
    }
}
