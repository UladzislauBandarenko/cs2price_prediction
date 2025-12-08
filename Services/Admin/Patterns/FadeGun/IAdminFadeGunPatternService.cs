using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Patterns.FadeGun;

namespace cs2price_prediction.Services.Admin.Patterns.FadeGun
{
    public interface IAdminFadeGunPatternService
    {
        Task<int> CreateAsync(CreateFadeGunPatternDto dto);
        Task<bool> UpdateAsync(int id, UpdateFadeGunPatternDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
