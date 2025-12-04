using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.WearTiers;
using cs2price_prediction.DTOs.Meta;

namespace cs2price_prediction.Services.Admin.WearTiers
{
    public interface IAdminWearTierService
    {
        Task<WearTierDto> CreateWearTierAsync(CreateWearTierDto dto);
        Task<WearTierDto?> UpdateWearTierAsync(int id, UpdateWearTierDto dto);
        Task<bool> DeleteWearTierAsync(int id);
    }
}
