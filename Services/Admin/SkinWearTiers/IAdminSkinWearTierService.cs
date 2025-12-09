using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.SkinWearTiers;

namespace cs2price_prediction.Services.Admin.SkinWearTiers
{
    public interface IAdminSkinWearTierService
    {
        Task<bool> CreateSkinWearTierAsync(CreateSkinWearTierDto dto);
        Task<bool> DeleteSkinWearTierAsync(int skinId, int wearTierId);
        Task<bool> UpdateSkinWearTierAsync(UpdateSkinWearTierDto dto);
    }
}
