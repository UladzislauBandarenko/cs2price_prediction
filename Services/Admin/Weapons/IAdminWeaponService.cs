using System.Threading.Tasks;
using cs2price_prediction.DTOs.Admin.Weapons;
using cs2price_prediction.DTOs.Meta;

namespace cs2price_prediction.Services.Admin.Weapons
{
    public interface IAdminWeaponService
    {
        Task<WeaponDto> CreateWeaponAsync(CreateWeaponDto dto);
        Task<WeaponDto?> UpdateWeaponAsync(int id, UpdateWeaponDto dto);
        Task<bool> DeleteWeaponAsync(int id);
    }
}
