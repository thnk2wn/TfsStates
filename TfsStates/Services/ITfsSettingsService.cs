using System.Threading.Tasks;
using TfsStates.Models;

namespace TfsStates.Services
{
    public interface ITfsSettingsService
    {
        Task<string> GetFilename();
        Task<TfsConnectionModel> GetSettingsOrDefault();
        Task<bool> HasSettings();
        Task<TfsConnectionModel> Save(TfsConnectionModel model, bool validate = true);
        Task<TfsConnectionModel> Validate(TfsConnectionModel model);
    }
}