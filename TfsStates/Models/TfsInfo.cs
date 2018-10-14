using System;
using Humanizer;

namespace TfsStates.Models
{
    public class TfsInfo
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Iteration { get; set; }

        public string State { get; set; }

        public int TransitionCount { get; set; }

        public string Transitions { get; set; }

        public DateTime? ClosedDate { get; set; }

        public string Priority { get; set; }

        public string Severity { get; set; }

        public string Tags { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Title} ({"transition".ToQuantity(TransitionCount)})";
        }
    }
}
