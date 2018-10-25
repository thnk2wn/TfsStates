using System;

namespace TfsStates.Models
{
    public class ReportRun
    {
        public string Project { get; set; }

        public string Iteration { get; set; }

        public Guid ConnectionId { get; set; }
    }
}
