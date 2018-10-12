using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.WebApi;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface ITfsSettingsService
    {
        Task<string> GetFilename();
        Task<TfsConnectionModel> GetSettings();
        Task<TfsConnectionModel> GetSettingsOrDefault();
        Task<VssConnection> GetConnection();
        Task<bool> HasSettings();
        Task<TfsConnectionModel> Save(TfsConnectionModel model, bool validate = true);
        Task<TfsConnectionModel> Validate(TfsConnectionModel model);
    }
}