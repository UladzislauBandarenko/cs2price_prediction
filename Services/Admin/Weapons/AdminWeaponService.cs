using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.Weapons;
using cs2price_prediction.DTOs.Meta;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.Weapons
{
    public class AdminWeaponService : IAdminWeaponService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminWeaponService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        public async Task<WeaponDto> CreateWeaponAsync(CreateWeaponDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var wtExists = await db.WeaponTypes.AnyAsync(x => x.Id == dto.WeaponTypeId);
            if (!wtExists)
                throw new ArgumentException("WeaponType not found", nameof(dto.WeaponTypeId));

            var entity = new Domain.Meta.Weapon
            {
                Name = dto.Name.Trim(),
                WeaponTypeId = dto.WeaponTypeId
            };

            db.Weapons.Add(entity);
            await db.SaveChangesAsync();

            return new WeaponDto(entity.Id, entity.Name);
        }

        public async Task<WeaponDto?> UpdateWeaponAsync(int id, UpdateWeaponDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.Weapons.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return null;

            var wtExists = await db.WeaponTypes.AnyAsync(x => x.Id == dto.WeaponTypeId);
            if (!wtExists)
                throw new ArgumentException("WeaponType not found", nameof(dto.WeaponTypeId));

            entity.Name = dto.Name.Trim();
            entity.WeaponTypeId = dto.WeaponTypeId;

            await db.SaveChangesAsync();
            return new WeaponDto(entity.Id, entity.Name);
        }

        public async Task<bool> DeleteWeaponAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.Weapons.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            db.Weapons.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
