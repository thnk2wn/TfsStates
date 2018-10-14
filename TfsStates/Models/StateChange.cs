using System;

namespace TfsStates.Models
{
    public class StateChange
    {
        public string State { get; set; }

        public string DurationText { get; set; }

        public string By { get; set; }

        public string Reason { get; set; }

        public DateTime StateChangeDate { get; set; }

        public string Summary
        {
            get
            {
                var stateDateText = StateChangeDate.ToString("G");

                if (string.IsNullOrEmpty(DurationText))
                {
                    return $"{State} by {By} on {stateDateText}";
                }

                return $"[{DurationText}] {State} ({Reason}) by {By} on {stateDateText}";
            }
        }
    }
}
