using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.FadeKnife;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Patterns.FadeKnife
{
    public class AdminFadeKnifePatternService : IAdminFadeKnifePatternService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminFadeKnifePatternService(IAdminDbContextFactory adminDbFactory)
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

        public async Task<int> CreateAsync(CreateFadeKnifePatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var skin = await db.Skins.FirstOrDefaultAsync(s => s.Id == dto.SkinId);
            if (skin is null)
                throw new ArgumentException("Skin not found", nameof(dto.SkinId));

            EnsureSkinPatternStyle(skin, "fade_knife");

            var exists = await db.FadeKnifePatterns
                .AnyAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (exists)
                throw new InvalidOperationException("Pattern already exists for this skin and pattern number.");

            var entity = new FadeKnifePattern
            {
                SkinId = dto.SkinId,
                Pattern = dto.Pattern,
                FadePercentage = dto.FadePercentage,
                FadeRank = dto.FadeRank
            };

            db.FadeKnifePatterns.Add(entity);
            await db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateFadeKnifePatternDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.FadeKnifePatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return false;

            EnsureSkinPatternStyle(entity.Skin, "fade_knife");

            entity.FadePercentage = dto.FadePercentage;
            entity.FadeRank = dto.FadeRank;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.FadeKnifePatterns.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            db.FadeKnifePatterns.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
