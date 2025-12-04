using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.WeaponTypes;
using cs2price_prediction.DTOs.Meta;

namespace cs2price_prediction.Services.Admin.WeaponTypes
{
    public interface IAdminWeaponTypeService
    {
        Task<WeaponTypeDto> CreateWeaponTypeAsync(CreateWeaponTypeDto dto);
        Task<WeaponTypeDto?> UpdateWeaponTypeAsync(int id, UpdateWeaponTypeDto dto);
        Task<bool> DeleteWeaponTypeAsync(int id);
    }
}
