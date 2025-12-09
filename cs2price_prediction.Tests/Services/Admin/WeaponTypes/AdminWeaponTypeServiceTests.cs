using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.WeaponTypes;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Services.Admin.WeaponTypes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.WeaponTypes
{
    public class AdminWeaponTypeServiceTests
    {
        private (AdminWeaponTypeService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();

            // Каждый вызов CreateAdminContext возвращает новый AppDbContext с теми же options
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminWeaponTypeService(factoryMock.Object);
            return (service, options);
        }

        [Fact]
        public async Task CreateWeaponTypeAsync_Creates_Entity_And_Returns_Dto()
        {
            // arrange
            var (service, options) = CreateService();

            var dto = new CreateWeaponTypeDto
            {
                Code = "  rifle  ",
                Name = "  Rifle  "
            };

            // act
            WeaponTypeDto result = await service.CreateWeaponTypeAsync(dto);

            // assert DTO
            result.Id.Should().BeGreaterThan(0);
            result.Code.Should().Be("rifle");
            result.Name.Should().Be("Rifle");

            // assert DB
            await using var db = new AppDbContext(options);
            var entity = await db.WeaponTypes.FirstOrDefaultAsync(w => w.Id == result.Id);
            entity.Should().NotBeNull();
            entity!.Code.Should().Be("rifle");
            entity.Name.Should().Be("Rifle");
        }

        [Fact]
        public async Task UpdateWeaponTypeAsync_ReturnsNull_When_Entity_Not_Found()
        {
            // arrange
            var (service, _) = CreateService();

            var dto = new UpdateWeaponTypeDto
            {
                Code = "any",
                Name = "Any"
            };

            // act
            WeaponTypeDto? result = await service.UpdateWeaponTypeAsync(999, dto);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateWeaponTypeAsync_Updates_Entity_And_Returns_Dto()
        {
            // arrange
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var wt = new WeaponType
                {
                    Code = "old_code",
                    Name = "Old Name"
                };
                seed.WeaponTypes.Add(wt);
                await seed.SaveChangesAsync();
                id = wt.Id;
            }

            var dto = new UpdateWeaponTypeDto
            {
                Code = "  new_code  ",
                Name = "  New Name  "
            };

            // act
            WeaponTypeDto? result = await service.UpdateWeaponTypeAsync(id, dto);

            // assert DTO
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Code.Should().Be("new_code");
            result.Name.Should().Be("New Name");

            // assert DB
            await using var db = new AppDbContext(options);
            var entity = await db.WeaponTypes.FirstOrDefaultAsync(w => w.Id == id);
            entity.Should().NotBeNull();
            entity!.Code.Should().Be("new_code");
            entity.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task DeleteWeaponTypeAsync_ReturnsFalse_When_Not_Found()
        {
            // arrange
            var (service, options) = CreateService();

            // База пустая
            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            // act
            bool deleted = await service.DeleteWeaponTypeAsync(123);

            // assert
            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteWeaponTypeAsync_Removes_Entity_And_ReturnsTrue()
        {
            // arrange
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var wt = new WeaponType
                {
                    Code = "smg",
                    Name = "SMG"
                };
                seed.WeaponTypes.Add(wt);
                await seed.SaveChangesAsync();
                id = wt.Id;
            }

            // act
            bool deleted = await service.DeleteWeaponTypeAsync(id);

            // assert флаг
            deleted.Should().BeTrue();

            // assert DB
            await using var db = new AppDbContext(options);
            var entity = await db.WeaponTypes.FirstOrDefaultAsync(w => w.Id == id);
            entity.Should().BeNull();
        }
    }
}
