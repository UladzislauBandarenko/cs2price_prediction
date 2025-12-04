using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.Doppler;

namespace cs2price_prediction.Services.Admin.Patterns.DopplerPhase
{
    public interface IAdminDopplerPhaseService
    {
        Task<int> CreateAsync(CreateDopplerPhaseDto dto);
        Task<bool> UpdateAsync(int id, UpdateDopplerPhaseDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
