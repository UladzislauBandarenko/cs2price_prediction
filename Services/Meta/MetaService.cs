using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Meta;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Meta
{
    public class MetaService : IMetaService
    {
        private readonly AppDbContext _db;

        public MetaService(AppDbContext db)
        {
            _db = db;
        }

        // Returns the list of all weapon types (sorted by ID)
        public async Task<IEnumerable<WeaponTypeDto>> GetWeaponTypesAsync()
        {
            return await _db.WeaponTypes
                .AsNoTracking()
                .OrderBy(wt => wt.Id)
                .Select(wt => new WeaponTypeDto(
                    wt.Id,
                    wt.Code,
                    wt.Name
                ))
                .ToListAsync();
        }

        // Returns all weapons for a specific weapon type
        public async Task<IEnumerable<WeaponDto>> GetWeaponsForTypeAsync(int weaponTypeId)
        {
            return await _db.Weapons
                .AsNoTracking()
                .Where(w => w.WeaponTypeId == weaponTypeId)
                .OrderBy(w => w.Name)
                .Select(w => new WeaponDto(
                    w.Id,
                    w.Name
                ))
                .ToListAsync();
        }

        // Returns all skins for a specific weapon
        public async Task<IEnumerable<SkinDto>> GetSkinsForWeaponAsync(int weaponId)
        {
            return await _db.Skins
                .AsNoTracking()
                .Where(s => s.WeaponId == weaponId)
                .OrderBy(s => s.Name)
                .Select(s => new SkinDto(
                    s.Id,
                    s.Name,
                    s.PatternStyle
                ))
                .ToListAsync();
        }

        // Returns all allowed wear tiers for a specific skin
        public async Task<IEnumerable<WearTierDto>> GetWearForSkinAsync(int skinId)
        {
            return await _db.SkinWearTiers
                .AsNoTracking()
                .Where(sw => sw.SkinId == skinId)
                .OrderBy(sw => sw.WearTier.Name)
                .Select(sw => new WearTierDto(
                    sw.WearTierId,
                    sw.WearTier.Name
                ))
                .ToListAsync();
        }

        // Returns available pattern options depending on the skin's pattern style.
        // If the skin does not exist → SkinExists = false.
        // If the skin exists but does not use patterns → empty list.
        public async Task<(bool SkinExists, IEnumerable<PatternOptionDto> Patterns)> GetPatternsForSkinAsync(int skinId)
        {
            var skin = await _db.Skins
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == skinId);

            // Skin not found
            if (skin is null)
                return (false, Enumerable.Empty<PatternOptionDto>());

            var style = skin.PatternStyle;

            switch (style)
            {
                // -------------------------------
                // Case-Hardened guns
                // -------------------------------
                case "ch_gun":
                    {
                        var patterns = await _db.CaseHardenedGunPatterns
                            .AsNoTracking()
                            .Where(p => p.SkinId == skinId)
                            .OrderBy(p => p.Pattern)
                            .Select(p => new PatternOptionDto(
                                p.Pattern,
                                p.Pattern.ToString()
                            ))
                            .ToListAsync();

                        return (true, patterns);
                    }

                // -------------------------------
                // Case-Hardened knives
                // -------------------------------
                case "ch_knife":
                    {
                        var patterns = await _db.CaseHardenedKnifePatterns
                            .AsNoTracking()
                            .Where(p => p.SkinId == skinId)
                            .OrderBy(p => p.Pattern)
                            .Select(p => new PatternOptionDto(
                                p.Pattern,
                                p.Pattern.ToString()
                            ))
                            .ToListAsync();

                        return (true, patterns);
                    }

                // -------------------------------
                // Fade guns
                // -------------------------------
                case "fade_gun":
                    {
                        var patterns = await _db.FadeGunPatterns
                            .AsNoTracking()
                            .Where(p => p.SkinId == skinId)
                            .OrderBy(p => p.Pattern)
                            .Select(p => new PatternOptionDto(
                                p.Pattern,
                                p.Pattern.ToString()
                            ))
                            .ToListAsync();

                        return (true, patterns);
                    }

                // -------------------------------
                // Fade knives
                // -------------------------------
                case "fade_knife":
                    {
                        var patterns = await _db.FadeKnifePatterns
                            .AsNoTracking()
                            .Where(p => p.SkinId == skinId)
                            .OrderBy(p => p.Pattern)
                            .Select(p => new PatternOptionDto(
                                p.Pattern,
                                p.Pattern.ToString()
                            ))
                            .ToListAsync();

                        return (true, patterns);
                    }

                // -------------------------------
                // Doppler knives (Pattern = PhaseId)
                // -------------------------------
                case "doppler_knife":
                    {
                        var phases = await _db.DopplerSkinPhases
                            .AsNoTracking()
                            .Where(d => d.SkinId == skinId)
                            .Include(d => d.Phase)
                            .OrderBy(d => d.Phase.Name)
                            .Select(d => new PatternOptionDto(
                                d.PhaseId,
                                d.Phase.Name
                            ))
                            .ToListAsync();

                        return (true, phases);
                    }

                // -------------------------------
                // Skins where pattern does not matter (float-only guns)
                // -------------------------------
                case "float_gun":
                default:
                    return (true, Enumerable.Empty<PatternOptionDto>());
            }
        }

        // Returns filtered list of stickers.
        // Supports case-insensitive search via ILIKE.
        public async Task<IEnumerable<StickerDto>> GetStickersAsync(string? q, int limit)
        {
            var query = _db.Stickers
                .AsNoTracking()
                .AsQueryable();

            // limits
            if (limit <= 0 || limit > 200)
                limit = 50;

            if (!string.IsNullOrWhiteSpace(q))
            {
                //  (PostgreSQL) 
                if (_db.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
                {
                    query = query.Where(s => EF.Functions.ILike(s.Name, $"%{q}%"));
                }
                else
                {
                    //  (InMemory) 
                    var qLower = q.ToLower();
                    query = query.Where(s => s.Name.ToLower().Contains(qLower));
                }
            }

            return await query
                .OrderBy(s => s.Name)
                .Take(limit)
                .Select(s => new StickerDto(
                    s.Id,
                    s.Name
                ))
                .ToListAsync();
        }

    }
}
