using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.Patterns.FadeGun;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Patterns.FadeGun
{
    public class AdminFadeGunPatternService : IAdminFadeGunPatternService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminFadeGunPatternService(IAdminDbContextFactory adminDbFactory)
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

        public async Task<int> CreateAsync(CreateFadeGunPatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var skin = await db.Skins.FirstOrDefaultAsync(s => s.Id == dto.SkinId);
            if (skin is null)
                throw new ArgumentException("Skin not found", nameof(dto.SkinId));

            EnsureSkinPatternStyle(skin, "fade_gun");

            var exists = await db.FadeGunPatterns
                .AnyAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (exists)
                throw new InvalidOperationException("Pattern already exists for this skin and pattern number.");

            var entity = new cs2price_prediction.Domain.Patterns.FadeGunPattern
            {
                SkinId = dto.SkinId,
                Pattern = dto.Pattern,
                FadePercentage = dto.FadePercentage,
                FadeRank = dto.FadeRank
            };

            db.FadeGunPatterns.Add(entity);
            await db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateFadeGunPatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.FadeGunPatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return false;

            EnsureSkinPatternStyle(entity.Skin, "fade_gun");

            entity.FadePercentage = dto.FadePercentage;
            entity.FadeRank = dto.FadeRank;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.FadeGunPatterns.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            db.FadeGunPatterns.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
