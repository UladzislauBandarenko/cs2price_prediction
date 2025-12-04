using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedGun;

namespace cs2price_prediction.Services.Admin.Patterns.CaseHardenedGun
{
    public interface IAdminCaseHardenedGunPatternService
    {
        Task<int> CreateAsync(CreateCaseHardenedGunPatternDto dto);
        Task<bool> UpdateAsync(int id, UpdateCaseHardenedGunPatternDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
