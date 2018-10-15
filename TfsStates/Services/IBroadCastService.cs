using TfsStates.Models;

namespace TfsStates.Services
{
    public interface IBroadcastService
    {
        void ReportProgress(ReportProgress progress);
    }
}