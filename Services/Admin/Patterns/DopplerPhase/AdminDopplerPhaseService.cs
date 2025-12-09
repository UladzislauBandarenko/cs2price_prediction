using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.Patterns.Doppler;
using Microsoft.EntityFrameworkCore;
using DomainDopplerPhase = cs2price_prediction.Domain.Patterns.DopplerPhase;

namespace cs2price_prediction.Services.Admin.Patterns.DopplerPhase
{
    public class AdminDopplerPhaseService : IAdminDopplerPhaseService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminDopplerPhaseService(IAdminDbContextFactory adminDbFactory)
        {
            _adminDbFactory = adminDbFactory;
        }

        public async Task<int> CreateAsync(CreateDopplerPhaseDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = new DomainDopplerPhase
            {
                Name = dto.Name.Trim()
            };

            db.DopplerPhases.Add(entity);
            await db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateDopplerPhaseDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.DopplerPhases.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            entity.Name = dto.Name.Trim();
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.DopplerPhases.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            db.DopplerPhases.Remove(entity);

            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // есть зависимые DopplerSkinPhases
                return false;
            }
        }
    }
}
