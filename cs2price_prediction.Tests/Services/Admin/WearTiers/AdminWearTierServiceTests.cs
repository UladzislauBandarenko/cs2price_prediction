using System;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.WearTiers;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Services.Admin.WearTiers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.WearTiers
{
    public class AdminWearTierServiceTests
    {
        private (AdminWearTierService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();

            // Каждый вызов CreateAdminContext возвращает новый контекст
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminWearTierService(factoryMock.Object);
            return (service, options);
        }

        [Fact]
        public async Task CreateWearTierAsync_Creates_Entity_And_Returns_Dto()
        {
            // arrange
            var (service, options) = CreateService();

            var dto = new CreateWearTierDto
            {
                Name = "  Field-Tested  "
            };

            // act
            WearTierDto result = await service.CreateWearTierAsync(dto);

            // assert dto
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Field-Tested");

            // assert DB
            await using var db = new AppDbContext(options);
            var entity = await db.WearTiers.FirstOrDefaultAsync(w => w.Id == result.Id);
            entity.Should().NotBeNull();
            entity!.Name.Should().Be("Field-Tested");
        }

        [Fact]
        public async Task UpdateWearTierAsync_ReturnsNull_When_Entity_Not_Found()
        {
            // arrange
            var (service, _) = CreateService();

            var dto = new UpdateWearTierDto
            {
                Name = "Anything"
            };

            // act
            WearTierDto? result = await service.UpdateWearTierAsync(999, dto);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateWearTierAsync_Updates_Name_And_Returns_Dto()
        {
            // arrange
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var wt = new WearTier { Name = "Old Name" };
                seed.WearTiers.Add(wt);
                await seed.SaveChangesAsync();
                id = wt.Id;
            }

            var dto = new UpdateWearTierDto
            {
                Name = "  New Name  "
            };

            // act
            WearTierDto? result = await service.UpdateWearTierAsync(id, dto);

            // assert dto
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("New Name");

            // assert DB
            await using var db = new AppDbContext(options);
            var entity = await db.WearTiers.FirstOrDefaultAsync(w => w.Id == id);
            entity.Should().NotBeNull();
            entity!.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task DeleteWearTierAsync_ReturnsFalse_When_Not_Found()
        {
            // arrange
            var (service, options) = CreateService();

            // в БД ничего нет
            await using (var seed = new AppDbContext(options))
            {
                // пусто
                await seed.SaveChangesAsync();
            }

            // act
            bool deleted = await service.DeleteWearTierAsync(123);

            // assert
            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteWearTierAsync_Removes_Entity_And_ReturnsTrue()
        {
            // arrange
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var wt = new WearTier { Name = "To Delete" };
                seed.WearTiers.Add(wt);
                await seed.SaveChangesAsync();
                id = wt.Id;
            }

            // act
            bool deleted = await service.DeleteWearTierAsync(id);

            // assert flag
            deleted.Should().BeTrue();

            // assert DB
            await using var db = new AppDbContext(options);
            (await db.WearTiers.CountAsync()).Should().Be(0);
        }
    }
}
