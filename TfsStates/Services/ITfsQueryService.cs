using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface ITfsQueryService
    {
        Task<TfsQueryResult> Query(TfsStatesModel model);
    }
}