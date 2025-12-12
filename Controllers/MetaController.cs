using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Meta;
using Microsoft.AspNetCore.Mvc;

namespace cs2price_prediction.Controllers
{
    [ApiController]
    [Route("api/meta")]
    public class MetaController : ControllerBase
    {
        private readonly IMetaService _metaService;

        public MetaController(IMetaService metaService)
        {
            _metaService = metaService;
        }

        // 1) Weapon types: rifle / pistol / smg / sniper / knife / ...
        //
        // GET /api/meta/weapon-types
        [HttpGet("weapon-types")]
        public async Task<IEnumerable<WeaponTypeDto>> GetWeaponTypes()
        {
            return await _metaService.GetWeaponTypesAsync();
        }

        // 2) Weapons for a specific weapon type
        //
        // GET /api/meta/weapon-types/{weaponTypeId}/weapons
        [HttpGet("weapon-types/{weaponTypeId:int}/weapons")]
        public async Task<IEnumerable<WeaponDto>> GetWeaponsForType(int weaponTypeId)
        {
            return await _metaService.GetWeaponsForTypeAsync(weaponTypeId);
        }

        // 3) All skins for a specific weapon (patternStyle is not passed in the request)
        //
        // GET /api/meta/weapons/{weaponId}/skins
        [HttpGet("weapons/{weaponId:int}/skins")]
        public async Task<IEnumerable<SkinDto>> GetSkinsForWeapon(int weaponId)
        {
            return await _metaService.GetSkinsForWeaponAsync(weaponId);
        }

        // 4) Available wear tiers (conditions) for a specific skin
        //
        // GET /api/meta/skins/{skinId}/wear-tiers
        [HttpGet("skins/{skinId:int}/wear-tiers")]
        public async Task<IEnumerable<WearTierDto>> GetWearForSkin(int skinId)
        {
            return await _metaService.GetWearForSkinAsync(skinId);
        }

        // 5) Pattern / phase options for a specific skin
        //
        // GET /api/meta/skins/{skinId}/patterns
        [HttpGet("skins/{skinId:int}/patterns")]
        public async Task<ActionResult<IEnumerable<PatternOptionDto>>> GetPatternsForSkin(int skinId)
        {
            var (skinExists, patterns) = await _metaService.GetPatternsForSkinAsync(skinId);

            if (!skinExists)
                return NotFound("Skin not found");

            return Ok(patterns);
        }

        // 6) Sticker search (for selecting up to 4 stickers)
        //
        // GET /api/meta/stickers?q=...&limit=50
        [HttpGet("stickers")]
        public async Task<IEnumerable<StickerDto>> GetStickers(
            [FromQuery] string? q = null,
            [FromQuery] int limit = 50)
        {
            return await _metaService.GetStickersAsync(q, limit);
        }
    }
}
