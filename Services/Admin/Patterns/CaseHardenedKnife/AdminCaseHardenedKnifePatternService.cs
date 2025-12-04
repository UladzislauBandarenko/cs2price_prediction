using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedKnife;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Patterns.CaseHardenedKnife
{
    public class AdminCaseHardenedKnifePatternService : IAdminCaseHardenedKnifePatternService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminCaseHardenedKnifePatternService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        private static void EnsureSkinPatternStyle(Skin skin, string expectedStyle)
        {
            if (!string.Equals(skin.PatternStyle, expectedStyle, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Skin {skin.Id} has patternStyle='{skin.PatternStyle}', expected '{expectedStyle}'.");
            }
        }

        public async Task<int> CreateAsync(CreateCaseHardenedKnifePatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var skin = await db.Skins.FirstOrDefaultAsync(s => s.Id == dto.SkinId);
            if (skin is null)
                throw new ArgumentException("Skin not found", nameof(dto.SkinId));

            EnsureSkinPatternStyle(skin, "ch_knife");

            var exists = await db.CaseHardenedKnifePatterns
                .AnyAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (exists)
                throw new InvalidOperationException("Pattern already exists for this skin and pattern number.");

            var entity = new CaseHardenedKnifePattern
            {
                SkinId = dto.SkinId,
                Pattern = dto.Pattern,
                BacksideBlue = dto.BacksideBlue,
                BacksidePurple = dto.BacksidePurple,
                BacksideGold = dto.BacksideGold,
                PlaysideBlue = dto.PlaysideBlue,
                PlaysidePurple = dto.PlaysidePurple,
                PlaysideGold = dto.PlaysideGold
            };

            db.CaseHardenedKnifePatterns.Add(entity);
            await db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateCaseHardenedKnifePatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.CaseHardenedKnifePatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return false;

            EnsureSkinPatternStyle(entity.Skin, "ch_knife");

            entity.BacksideBlue = dto.BacksideBlue;
            entity.BacksidePurple = dto.BacksidePurple;
            entity.BacksideGold = dto.BacksideGold;
            entity.PlaysideBlue = dto.PlaysideBlue;
            entity.PlaysidePurple = dto.PlaysidePurple;
            entity.PlaysideGold = dto.PlaysideGold;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.CaseHardenedKnifePatterns.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            db.CaseHardenedKnifePatterns.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
