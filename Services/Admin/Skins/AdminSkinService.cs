using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.Skins;
using cs2price_prediction.DTOs.Meta;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Skins
{
    public class AdminSkinService : IAdminSkinService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminSkinService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        public async Task<SkinDto> CreateSkinAsync(CreateSkinDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var weaponExists = await db.Weapons.AnyAsync(w => w.Id == dto.WeaponId);
            if (!weaponExists)
                throw new ArgumentException("Weapon not found", nameof(dto.WeaponId));

            var entity = new Domain.Meta.Skin
            {
                WeaponId = dto.WeaponId,
                Name = dto.Name.Trim(),
                PatternStyle = dto.PatternStyle.Trim()
            };

            db.Skins.Add(entity);
            await db.SaveChangesAsync();

            return new SkinDto(entity.Id, entity.Name, entity.PatternStyle);
        }

        public async Task<SkinDto?> UpdateSkinAsync(int id, UpdateSkinDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.Skins.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null)
                return null;

            var weaponExists = await db.Weapons.AnyAsync(w => w.Id == dto.WeaponId);
            if (!weaponExists)
                throw new ArgumentException("Weapon not found", nameof(dto.WeaponId));

            var oldStyle = entity.PatternStyle;
            var newStyle = dto.PatternStyle?.Trim() ?? oldStyle;

            if (!string.Equals(oldStyle, newStyle, StringComparison.OrdinalIgnoreCase))
            {
                var hasChGun = await db.CaseHardenedGunPatterns.AnyAsync(p => p.SkinId == id);
                var hasChKnife = await db.CaseHardenedKnifePatterns.AnyAsync(p => p.SkinId == id);
                var hasFadeGun = await db.FadeGunPatterns.AnyAsync(p => p.SkinId == id);
                var hasFadeKnife = await db.FadeKnifePatterns.AnyAsync(p => p.SkinId == id);
                var hasDoppler = await db.DopplerSkinPhases.AnyAsync(p => p.SkinId == id);

                if (hasChGun || hasChKnife || hasFadeGun || hasFadeKnife || hasDoppler)
                {
                    throw new InvalidOperationException(
                        "Cannot change PatternStyle for skin that already has pattern records.");
                }

                entity.PatternStyle = newStyle;
            }

            entity.WeaponId = dto.WeaponId;
            entity.Name = dto.Name.Trim();

            await db.SaveChangesAsync();

            return new SkinDto(entity.Id, entity.Name, entity.PatternStyle);
        }

        public async Task<bool> DeleteSkinAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.Skins.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            db.Skins.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
