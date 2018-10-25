using System.Threading.Tasks;
using TfsStates.Models;
using TfsStates.ViewModels;

namespace TfsStates.Services
{
    public interface ITfsQueryService
    {
        Task<TfsQueryResult> Query(TfsStatesViewModel model);
    }
}