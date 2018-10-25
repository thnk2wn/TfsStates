using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface ITfsSettingsService
    {
        Task<string> GetFilename();

        Task<TfsKnownConnections> GetConnections();

        Task<TfsKnownConnections> GetConnectionsOrDefault();

        Task<TfsKnownConnection> GetActiveConnection();

        Task<bool> HasSettings();

        Task Save(TfsKnownConnection connection);

        Task<TfsConnectionValidationResult> Validate(TfsKnownConnection connection);
    }
}