using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.DopplerSkin;

namespace cs2price_prediction.Services.Admin.Patterns.DopplerSkinPhase
{
    public interface IAdminDopplerSkinPhaseService
    {
        Task<int> CreateAsync(CreateDopplerSkinPhaseDto dto);
        Task<bool> UpdateAsync(int id, UpdateDopplerSkinPhaseDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
