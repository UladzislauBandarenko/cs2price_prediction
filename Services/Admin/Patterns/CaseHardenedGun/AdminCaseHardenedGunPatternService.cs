using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedGun;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Patterns.CaseHardenedGun
{
    public class AdminCaseHardenedGunPatternService : IAdminCaseHardenedGunPatternService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminCaseHardenedGunPatternService(IAdminDbContextFactory adminDbFactory)
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

        public async Task<int> CreateAsync(CreateCaseHardenedGunPatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var skin = await db.Skins.FirstOrDefaultAsync(s => s.Id == dto.SkinId);
            if (skin is null)
                throw new ArgumentException("Skin not found", nameof(dto.SkinId));

            EnsureSkinPatternStyle(skin, "ch_gun");

            var exists = await db.CaseHardenedGunPatterns
                .AnyAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (exists)
                throw new InvalidOperationException("Pattern already exists for this skin and pattern number.");

            var entity = new cs2price_prediction.Domain.Patterns.CaseHardenedGunPattern
            {
                SkinId = dto.SkinId,
                Pattern = dto.Pattern,
                PlaysideBlue = dto.PlaysideBlue,
                BacksideBlue = dto.BacksideBlue
            };

            db.CaseHardenedGunPatterns.Add(entity);
            await db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateCaseHardenedGunPatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.CaseHardenedGunPatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return false;

            EnsureSkinPatternStyle(entity.Skin, "ch_gun");

            entity.PlaysideBlue = dto.PlaysideBlue;
            entity.BacksideBlue = dto.BacksideBlue;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.CaseHardenedGunPatterns.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            db.CaseHardenedGunPatterns.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
