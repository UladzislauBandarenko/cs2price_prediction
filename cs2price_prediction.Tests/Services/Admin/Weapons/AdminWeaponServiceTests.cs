using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Admin.Weapons;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Services.Admin.Weapons;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.Weapons
{
    public class AdminWeaponServiceTests
    {
        private (AdminWeaponService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();

            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminWeaponService(factoryMock.Object);
            return (service, options);
        }

        // ----------------- CreateWeaponAsync -----------------

        [Fact]
        public async Task CreateWeaponAsync_Throws_When_WeaponType_Not_Found()
        {
            var (service, options) = CreateService();

            // В БД нет WeaponTypes
            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var dto = new CreateWeaponDto
            {
                Name = "AK-47",
                WeaponTypeId = 999
            };

            Func<Task> act = async () => await service.CreateWeaponAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.WeaponTypeId));
            ex.And.Message.Should().Contain("WeaponType not found");
        }

        [Fact]
        public async Task CreateWeaponAsync_Creates_Weapon_When_WeaponType_Exists()
        {
            var (service, options) = CreateService();

            int wtId;
            await using (var seed = new AppDbContext(options))
            {
                var wt = new WeaponType { Code = "rifle", Name = "Rifle" };
                seed.WeaponTypes.Add(wt);
                await seed.SaveChangesAsync();
                wtId = wt.Id;
            }

            var dto = new CreateWeaponDto
            {
                Name = "  AK-47  ",
                WeaponTypeId = wtId
            };

            WeaponDto result = await service.CreateWeaponAsync(dto);

            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("AK-47");

            await using var db = new AppDbContext(options);
            var entity = await db.Weapons.FirstOrDefaultAsync(w => w.Id == result.Id);
            entity.Should().NotBeNull();
            entity!.Name.Should().Be("AK-47");
            entity.WeaponTypeId.Should().Be(wtId);
        }

        // ----------------- UpdateWeaponAsync -----------------

        [Fact]
        public async Task UpdateWeaponAsync_ReturnsNull_When_Weapon_Not_Found()
        {
            var (service, options) = CreateService();

            // seed только WeaponType, но без Weapon
            await using (var seed = new AppDbContext(options))
            {
                seed.WeaponTypes.Add(new WeaponType { Code = "rifle", Name = "Rifle" });
                await seed.SaveChangesAsync();
            }

            var dto = new UpdateWeaponDto
            {
                Name = "New Name",
                WeaponTypeId = 1
            };

            WeaponDto? result = await service.UpdateWeaponAsync(999, dto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateWeaponAsync_Throws_When_WeaponType_Not_Found()
        {
            var (service, options) = CreateService();

            int weaponId;
            await using (var seed = new AppDbContext(options))
            {
                // существует оружие, но подходящего WeaponType под dto.WeaponTypeId нет
                var wt = new WeaponType { Code = "rifle", Name = "Rifle" };
                seed.WeaponTypes.Add(wt);

                var weapon = new Weapon
                {
                    Name = "Old Weapon",
                    WeaponTypeId = wt.Id
                };
                seed.Weapons.Add(weapon);

                await seed.SaveChangesAsync();
                weaponId = weapon.Id;
            }

            var dto = new UpdateWeaponDto
            {
                Name = "Any",
                WeaponTypeId = 999 // несуществующий тип
            };

            Func<Task> act = async () => await service.UpdateWeaponAsync(weaponId, dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.WeaponTypeId));
            ex.And.Message.Should().Contain("WeaponType not found");
        }

        [Fact]
        public async Task UpdateWeaponAsync_Updates_Weapon_When_All_Ok()
        {
            var (service, options) = CreateService();

            int weaponId;
            int oldTypeId;
            int newTypeId;

            await using (var seed = new AppDbContext(options))
            {
                var wtOld = new WeaponType { Code = "rifle", Name = "Rifle" };
                var wtNew = new WeaponType { Code = "smg", Name = "SMG" };
                seed.WeaponTypes.AddRange(wtOld, wtNew);

                var weapon = new Weapon
                {
                    Name = "Old Weapon",
                    WeaponTypeId = wtOld.Id
                };

                seed.Weapons.Add(weapon);
                await seed.SaveChangesAsync();

                weaponId = weapon.Id;
                oldTypeId = wtOld.Id;
                newTypeId = wtNew.Id;
            }

            var dto = new UpdateWeaponDto
            {
                Name = "  New Weapon Name  ",
                WeaponTypeId = newTypeId
            };

            WeaponDto? result = await service.UpdateWeaponAsync(weaponId, dto);

            result.Should().NotBeNull();
            result!.Id.Should().Be(weaponId);
            result.Name.Should().Be("New Weapon Name");

            await using var db = new AppDbContext(options);
            var entity = await db.Weapons.FirstOrDefaultAsync(w => w.Id == weaponId);
            entity.Should().NotBeNull();
            entity!.Name.Should().Be("New Weapon Name");
            entity.WeaponTypeId.Should().Be(newTypeId);
        }

        // ----------------- DeleteWeaponAsync -----------------

        [Fact]
        public async Task DeleteWeaponAsync_ReturnsFalse_When_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            bool deleted = await service.DeleteWeaponAsync(123);

            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteWeaponAsync_Removes_Weapon_And_ReturnsTrue()
        {
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var wt = new WeaponType { Code = "rifle", Name = "Rifle" };
                seed.WeaponTypes.Add(wt);

                var weapon = new Weapon
                {
                    Name = "To Delete",
                    WeaponTypeId = wt.Id
                };

                seed.Weapons.Add(weapon);
                await seed.SaveChangesAsync();

                id = weapon.Id;
            }

            bool deleted = await service.DeleteWeaponAsync(id);

            deleted.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.Weapons.FirstOrDefaultAsync(w => w.Id == id);
            entity.Should().BeNull();
        }
    }
}
