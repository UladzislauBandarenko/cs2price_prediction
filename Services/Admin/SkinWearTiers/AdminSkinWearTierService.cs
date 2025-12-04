using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.SkinWearTiers;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.SkinWearTiers
{
    public class AdminSkinWearTierService : IAdminSkinWearTierService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminSkinWearTierService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        public async Task<bool> CreateSkinWearTierAsync(CreateSkinWearTierDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var exists = await db.SkinWearTiers
                .AnyAsync(sw => sw.SkinId == dto.SkinId && sw.WearTierId == dto.WearTierId);

            if (exists)
                return false;

            var skinExists = await db.Skins.AnyAsync(s => s.Id == dto.SkinId);
            if (!skinExists)
                throw new ArgumentException("Skin not found", nameof(dto.SkinId));

            var wearExists = await db.WearTiers.AnyAsync(w => w.Id == dto.WearTierId);
            if (!wearExists)
                throw new ArgumentException("WearTier not found", nameof(dto.WearTierId));

            var entity = new Domain.Meta.SkinWearTier
            {
                SkinId = dto.SkinId,
                WearTierId = dto.WearTierId
            };

            db.SkinWearTiers.Add(entity);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSkinWearTierAsync(int skinId, int wearTierId)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.SkinWearTiers
                .FirstOrDefaultAsync(sw => sw.SkinId == skinId && sw.WearTierId == wearTierId);

            if (entity is null)
                return false;

            db.SkinWearTiers.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSkinWearTierAsync(UpdateSkinWearTierDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.SkinWearTiers
                .FirstOrDefaultAsync(sw => sw.SkinId == dto.SkinId &&
                                           sw.WearTierId == dto.OldWearTierId);

            if (entity is null)
                return false; 

            var wearExists = await db.WearTiers.AnyAsync(w => w.Id == dto.NewWearTierId);
            if (!wearExists)
                throw new ArgumentException("New WearTier not found", nameof(dto.NewWearTierId));

            var duplicate = await db.SkinWearTiers
                .AnyAsync(sw => sw.SkinId == dto.SkinId &&
                                sw.WearTierId == dto.NewWearTierId);

            if (duplicate)
                throw new InvalidOperationException("SkinWearTier with the new WearTier already exists.");

            entity.WearTierId = dto.NewWearTierId;

            await db.SaveChangesAsync();
            return true;
        }
    }
}
