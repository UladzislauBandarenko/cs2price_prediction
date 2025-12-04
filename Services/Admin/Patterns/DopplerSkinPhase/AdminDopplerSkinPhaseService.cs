using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.Patterns.DopplerSkin;
using Microsoft.EntityFrameworkCore;

// alias на доменную сущность DopplerSkinPhase
using DomainDopplerSkinPhase = cs2price_prediction.Domain.Patterns.DopplerSkinPhase;

namespace cs2price_prediction.Services.Admin.Patterns.DopplerSkinPhase
{
    public class AdminDopplerSkinPhaseService : IAdminDopplerSkinPhaseService
    {
        private readonly IAdminDbContextFactory _adminDbFactory;

        public AdminDopplerSkinPhaseService(IAdminDbContextFactory adminDbFactory)
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

        public async Task<int> CreateAsync(CreateDopplerSkinPhaseDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var skin = await db.Skins.FirstOrDefaultAsync(s => s.Id == dto.SkinId);
            if (skin is null)
                throw new ArgumentException("Skin not found", nameof(dto.SkinId));

            EnsureSkinPatternStyle(skin, "doppler_knife");

            var phaseExists = await db.DopplerPhases.AnyAsync(p => p.Id == dto.PhaseId);
            if (!phaseExists)
                throw new ArgumentException("Phase not found", nameof(dto.PhaseId));

            var exists = await db.DopplerSkinPhases
                .AnyAsync(p => p.SkinId == dto.SkinId && p.PhaseId == dto.PhaseId);

            if (exists)
                throw new InvalidOperationException("This skin already has this doppler phase.");

            var entity = new DomainDopplerSkinPhase
            {
                SkinId = dto.SkinId,
                PhaseId = dto.PhaseId
            };

            db.DopplerSkinPhases.Add(entity);
            await db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateDopplerSkinPhaseDto dto)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.DopplerSkinPhases
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity is null)
                return false;

            EnsureSkinPatternStyle(entity.Skin, "doppler_knife");

            var phaseExists = await db.DopplerPhases.AnyAsync(p => p.Id == dto.PhaseId);
            if (!phaseExists)
                throw new ArgumentException("Phase not found", nameof(dto.PhaseId));

            var exists = await db.DopplerSkinPhases
                .AnyAsync(p => p.SkinId == entity.SkinId && p.PhaseId == dto.PhaseId && p.Id != id);

            if (exists)
                throw new InvalidOperationException("This skin already has this doppler phase.");

            entity.PhaseId = dto.PhaseId;
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _adminDbFactory.CreateAdminContext();

            var entity = await db.DopplerSkinPhases.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null)
                return false;

            db.DopplerSkinPhases.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
