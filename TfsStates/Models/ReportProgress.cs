using System;

namespace TfsStates.Models
{
    public class ReportProgress
    {
        public string Message { get; set; }

        public int? WorkItemsProcessed { get; set; }

        public int? TotalCount { get; set; }

        public int? PercentDone 
        {
            get
            {
                if (!this.WorkItemsProcessed.HasValue || !this.TotalCount.HasValue || this.TotalCount == 0)
                {
                    return null;
                }

                var per = this.WorkItemsProcessed.Value / (double)this.TotalCount.Value * 100;
                return Convert.ToInt32(per);
            } 
        }
    }
}
