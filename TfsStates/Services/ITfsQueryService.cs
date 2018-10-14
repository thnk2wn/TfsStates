using System.Collections.Generic;
using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface ITfsQueryService
    {
        Task<List<TfsInfo>> Query(TfsStatesModel model);
    }
}