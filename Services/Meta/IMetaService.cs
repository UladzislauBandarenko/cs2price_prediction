using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Meta;

namespace cs2price_prediction.Services.Meta
{
    public interface IMetaService
    {
        Task<IEnumerable<WeaponTypeDto>> GetWeaponTypesAsync();
        Task<IEnumerable<WeaponDto>> GetWeaponsForTypeAsync(int weaponTypeId);
        Task<IEnumerable<SkinDto>> GetSkinsForWeaponAsync(int weaponId);
        Task<IEnumerable<WearTierDto>> GetWearForSkinAsync(int skinId);

        /// <summary>
        /// SkinExists = false, если скина нет → контроллер вернёт 404.
        /// </summary>
        Task<(bool SkinExists, IEnumerable<PatternOptionDto> Patterns)> GetPatternsForSkinAsync(int skinId);

        Task<IEnumerable<StickerDto>> GetStickersAsync(string? q, int limit);
    }
}
