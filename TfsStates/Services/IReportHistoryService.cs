using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface IReportHistoryService
    {
        Task<ReportRun> GetLastRunSettings();
        Task Record(TfsStatesModel model);
        Task Clear();
    }
}