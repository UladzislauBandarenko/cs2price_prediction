using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.WearTiers;
using cs2price_prediction.DTOs.Meta;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Admin.WearTiers
{
    public class AdminWearTierService : IAdminWearTierService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminWearTierService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        public async Task<WearTierDto> CreateWearTierAsync(CreateWearTierDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = new Domain.Meta.WearTier
            {
                Name = dto.Name.Trim()
            };

            db.WearTiers.Add(entity);
            await db.SaveChangesAsync();

            return new WearTierDto(entity.Id, entity.Name);
        }

        public async Task<WearTierDto?> UpdateWearTierAsync(int id, UpdateWearTierDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.WearTiers.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return null;

            entity.Name = dto.Name.Trim();
            await db.SaveChangesAsync();

            return new WearTierDto(entity.Id, entity.Name);
        }

        public async Task<bool> DeleteWearTierAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.WearTiers.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            db.WearTiers.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
