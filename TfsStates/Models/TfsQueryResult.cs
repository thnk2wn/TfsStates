using System.Collections.Generic;

namespace TfsStates.Models
{
    public class TfsQueryResult
    {
        public List<TfsInfo> TfsItems { get; set; }

        public int TotalWorkItems { get; set; }

        public int TotalRevisions { get; set; }
    }
}
