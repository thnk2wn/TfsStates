using System;
using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface IReportHistoryService
    {
        Task<ReportRun> GetLastRunSettings();

        Task Record(Guid knownConnId, string project, string iteration);

        Task Clear();
    }
}