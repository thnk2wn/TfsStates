using System;
using System.Collections.Generic;
using System.Linq;

namespace TfsStates.Models
{
    public class TfsKnownConnections
    {
        public Guid? ActiveConnectionId { get; set; }

        public List<TfsKnownConnection> Connections { get; set; }

        public TfsKnownConnection GetActiveConnection()
        {
            if (Connections == null || !Connections.Any())
            { 
                return null;
            }

            // TODO: Name for connection
            return Connections.FirstOrDefault(c => c.Id == ActiveConnectionId);
        }
    }
}
