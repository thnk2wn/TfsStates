using System.Collections.Generic;
using System.Threading.Tasks;

namespace TfsStates.Services
{
    public interface ITfsProjectService
    {
        Task<List<string>> GetProjectNames();
    }
}