using System;
using System.Collections.Generic;
using System.Linq;

namespace TfsStates.Models
{
    public class TfsKnownConnections
    {
        public TfsKnownConnections()
        {
            Connections = new List<TfsKnownConnection>();
        }

        public List<TfsKnownConnection> Connections { get; set; }

        public TfsKnownConnection Connection(Guid connectionId)
        {
            if (Connections == null) return null;
            return Connections.FirstOrDefault(c => c.Id == connectionId);
        }
    }
}
