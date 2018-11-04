using System.Collections.Generic;
using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface ITfsProjectService
    {
        Task<List<string>> GetProjectNames(TfsKnownConnection knownConn);

        Task<List<string>> GetIterations(TfsKnownConnection knownConn, string projectName);
    }
}