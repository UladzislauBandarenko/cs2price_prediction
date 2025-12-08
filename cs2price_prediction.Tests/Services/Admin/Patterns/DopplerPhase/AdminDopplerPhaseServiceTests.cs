using System;
using System.Threading;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.Patterns.Doppler;
using cs2price_prediction.Services.Admin.Patterns.DopplerPhase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// alias на доменную сущность DopplerPhase (как в сервисе)
using DomainDopplerPhase = cs2price_prediction.Domain.Patterns.DopplerPhase;

namespace cs2price_prediction.Tests.Services.Admin.Patterns
{
    public class AdminDopplerPhaseServiceTests
    {
        private (AdminDopplerPhaseService service, DbContextOptions<AppDbContext> options, Mock<IAdminDbContextFactory> factoryMock)
            CreateServiceWithFactory(Func<AppDbContext> contextFactory = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();

            if (contextFactory == null)
            {
                contextFactory = () => new AppDbContext(options);
            }

            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(contextFactory);

            var service = new AdminDopplerPhaseService(factoryMock.Object);
            return (service, options, factoryMock);
        }

        // ----------------- CreateAsync -----------------

        [Fact]
        public async Task CreateAsync_Creates_DopplerPhase_With_Trimmed_Name()
        {
            var (service, options, _) = CreateServiceWithFactory();

            var dto = new CreateDopplerPhaseDto
            {
                Name = "  Phase 1  "
            };

            int id = await service.CreateAsync(dto);

            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var entity = await db.DopplerPhases.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.Name.Should().Be("Phase 1");
        }

        // ----------------- UpdateAsync -----------------

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_When_Entity_Not_Found()
        {
            var (service, options, _) = CreateServiceWithFactory();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var dto = new UpdateDopplerPhaseDto
            {
                Name = "New Name"
            };

            bool result = await service.UpdateAsync(123, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Updates_Name_And_ReturnsTrue()
        {
            var (service, options, _) = CreateServiceWithFactory();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var phase = new DomainDopplerPhase
                {
                    Name = "Old Name"
                };
                seed.DopplerPhases.Add(phase);
                await seed.SaveChangesAsync();
                id = phase.Id;
            }

            var dto = new UpdateDopplerPhaseDto
            {
                Name = "  New Phase Name  "
            };

            bool result = await service.UpdateAsync(id, dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.DopplerPhases.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.Name.Should().Be("New Phase Name");
        }

        // ----------------- DeleteAsync -----------------

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_When_Entity_Not_Found()
        {
            var (service, options, _) = CreateServiceWithFactory();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            bool result = await service.DeleteAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Removes_Entity_And_ReturnsTrue_When_Save_Succeeds()
        {
            var (service, options, _) = CreateServiceWithFactory();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var phase = new DomainDopplerPhase
                {
                    Name = "To Delete"
                };
                seed.DopplerPhases.Add(phase);
                await seed.SaveChangesAsync();
                id = phase.Id;
            }

            bool result = await service.DeleteAsync(id);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.DopplerPhases.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().BeNull();
        }

        // Контекст, который всегда кидает DbUpdateException при SaveChangesAsync
        private class ThrowingAppDbContext : AppDbContext
        {
            public ThrowingAppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {
            }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new DbUpdateException("Simulated FK violation", (Exception?)null);
            }
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_When_SaveChanges_Throws_DbUpdateException()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // сначала засеваем фазу обычным контекстом
            await using (var seed = new AppDbContext(options))
            {
                seed.DopplerPhases.Add(new DomainDopplerPhase
                {
                    Id = 1,
                    Name = "Protected Phase"
                });
                await seed.SaveChangesAsync();
            }

            // фабрика, которая возвращает ThrowingAppDbContext
            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new ThrowingAppDbContext(options));

            var service = new AdminDopplerPhaseService(factoryMock.Object);

            bool result = await service.DeleteAsync(1);

            result.Should().BeFalse();
        }
    }
}
