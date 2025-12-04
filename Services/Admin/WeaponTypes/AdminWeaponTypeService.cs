using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.WeaponTypes;
using cs2price_prediction.DTOs.Meta;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.WeaponTypes
{
    public class AdminWeaponTypeService : IAdminWeaponTypeService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminWeaponTypeService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        public async Task<WeaponTypeDto> CreateWeaponTypeAsync(CreateWeaponTypeDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = new Domain.Meta.WeaponType
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim()
            };

            db.WeaponTypes.Add(entity);
            await db.SaveChangesAsync();

            return new WeaponTypeDto(entity.Id, entity.Code, entity.Name);
        }

        public async Task<WeaponTypeDto?> UpdateWeaponTypeAsync(int id, UpdateWeaponTypeDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.WeaponTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return null;

            entity.Code = dto.Code.Trim();
            entity.Name = dto.Name.Trim();

            await db.SaveChangesAsync();
            return new WeaponTypeDto(entity.Id, entity.Code, entity.Name);
        }

        public async Task<bool> DeleteWeaponTypeAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.WeaponTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            db.WeaponTypes.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
