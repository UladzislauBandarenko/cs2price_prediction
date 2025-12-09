using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.Admin.Skins;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.Skins;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.Skins
{
    public class AdminSkinServiceTests
    {
        private (AdminSkinService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminSkinService(factoryMock.Object);
            return (service, options);
        }

        // ----------------- CreateSkinAsync -----------------

        [Fact]
        public async Task CreateSkinAsync_Throws_When_Weapon_Not_Found()
        {
            var (service, options) = CreateService();

            // БД пустая, оружия нет
            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var dto = new CreateSkinDto
            {
                WeaponId = 123,
                Name = "Test Skin",
                PatternStyle = "float_gun"
            };

            Func<Task> act = async () => await service.CreateSkinAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.WeaponId));
            ex.And.Message.Should().Contain("Weapon not found");
        }

        [Fact]
        public async Task CreateSkinAsync_Creates_Skin_When_Weapon_Exists()
        {
            var (service, options) = CreateService();

            int weaponId;
            await using (var seed = new AppDbContext(options))
            {
                var weapon = new Weapon
                {
                    Name = "AK-47",
                    WeaponTypeId = 1
                };
                seed.Weapons.Add(weapon);
                await seed.SaveChangesAsync();
                weaponId = weapon.Id;
            }

            var dto = new CreateSkinDto
            {
                WeaponId = weaponId,
                Name = "  Redline  ",
                PatternStyle = "  float_gun  "
            };

            SkinDto result = await service.CreateSkinAsync(dto);

            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Redline");
            result.PatternStyle.Should().Be("float_gun");

            await using var db = new AppDbContext(options);
            var entity = await db.Skins.FirstOrDefaultAsync(s => s.Id == result.Id);
            entity.Should().NotBeNull();
            entity!.WeaponId.Should().Be(weaponId);
            entity.Name.Should().Be("Redline");
            entity.PatternStyle.Should().Be("float_gun");
        }

        // ----------------- UpdateSkinAsync -----------------

        [Fact]
        public async Task UpdateSkinAsync_ReturnsNull_When_Skin_Not_Found()
        {
            var (service, options) = CreateService();

            // seed только weapon
            await using (var seed = new AppDbContext(options))
            {
                seed.Weapons.Add(new Weapon { Name = "AK-47", WeaponTypeId = 1 });
                await seed.SaveChangesAsync();
            }

            var dto = new UpdateSkinDto
            {
                WeaponId = 1,
                Name = "Any",
                PatternStyle = "float_gun"
            };

            SkinDto? result = await service.UpdateSkinAsync(999, dto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateSkinAsync_Throws_When_Weapon_Not_Found()
        {
            var (service, options) = CreateService();

            int skinId;
            await using (var seed = new AppDbContext(options))
            {
                var weapon = new Weapon { Name = "AK-47", WeaponTypeId = 1 };
                seed.Weapons.Add(weapon);

                var skin = new Skin
                {
                    Name = "Old Skin",
                    Weapon = weapon,
                    WeaponId = weapon.Id,
                    PatternStyle = "float_gun"
                };
                seed.Skins.Add(skin);

                await seed.SaveChangesAsync();
                skinId = skin.Id;
            }

            var dto = new UpdateSkinDto
            {
                WeaponId = 999,           // несуществующее оружие
                Name = "New Skin",
                PatternStyle = "float_gun"
            };

            Func<Task> act = async () => await service.UpdateSkinAsync(skinId, dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.WeaponId));
            ex.And.Message.Should().Contain("Weapon not found");
        }

        [Fact]
        public async Task UpdateSkinAsync_DoesNot_Check_Patterns_When_Style_Not_Changed()
        {
            var (service, options) = CreateService();

            int skinId;
            int weaponId;
            await using (var seed = new AppDbContext(options))
            {
                var weapon = new Weapon { Name = "AK-47", WeaponTypeId = 1 };
                seed.Weapons.Add(weapon);
                await seed.SaveChangesAsync();
                weaponId = weapon.Id;

                var skin = new Skin
                {
                    Name = "Old Skin",
                    WeaponId = weaponId,
                    PatternStyle = "float_gun"
                };
                seed.Skins.Add(skin);

                // даже если в БД есть какие-то паттерны, но стиль не меняем —
                // код не должен падать
                seed.CaseHardenedGunPatterns.Add(new CaseHardenedGunPattern
                {
                    SkinId = skin.Id,
                    Pattern = 1
                });

                await seed.SaveChangesAsync();
                skinId = skin.Id;
            }

            var dto = new UpdateSkinDto
            {
                WeaponId = weaponId,
                Name = "  New Name  ",
                PatternStyle = "FLOAT_GUN" // тот же стиль, но другой регистр
            };

            var result = await service.UpdateSkinAsync(skinId, dto);

            result.Should().NotBeNull();
            result!.Name.Should().Be("New Name");
            result.PatternStyle.Should().Be("float_gun");

            await using var db = new AppDbContext(options);
            var updated = await db.Skins.FirstOrDefaultAsync(s => s.Id == skinId);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("New Name");
            updated.PatternStyle.Should().Be("float_gun");
        }

        [Fact]
        public async Task UpdateSkinAsync_Throws_When_Changing_Style_And_Patterns_Exist()
        {
            var (service, options) = CreateService();

            int skinId;
            int weaponId;
            await using (var seed = new AppDbContext(options))
            {
                var weapon = new Weapon { Name = "AK-47", WeaponTypeId = 1 };
                seed.Weapons.Add(weapon);
                await seed.SaveChangesAsync();
                weaponId = weapon.Id;

                var skin = new Skin
                {
                    Name = "Patterned Skin",
                    WeaponId = weaponId,
                    PatternStyle = "ch_gun"
                };
                seed.Skins.Add(skin);
                await seed.SaveChangesAsync();
                skinId = skin.Id;

                seed.CaseHardenedGunPatterns.Add(new CaseHardenedGunPattern
                {
                    SkinId = skinId,
                    Pattern = 123
                });

                await seed.SaveChangesAsync();
            }

            var dto = new UpdateSkinDto
            {
                WeaponId = weaponId,
                Name = "Updated",
                PatternStyle = "float_gun" // новый стиль
            };

            Func<Task> act = async () => await service.UpdateSkinAsync(skinId, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot change PatternStyle for skin that already has pattern records.");
        }

        [Fact]
        public async Task UpdateSkinAsync_Updates_Style_When_No_Patterns()
        {
            var (service, options) = CreateService();

            int skinId;
            int weaponId;
            await using (var seed = new AppDbContext(options))
            {
                var weapon = new Weapon { Name = "AK-47", WeaponTypeId = 1 };
                seed.Weapons.Add(weapon);
                await seed.SaveChangesAsync();
                weaponId = weapon.Id;

                var skin = new Skin
                {
                    Name = "Style Skin",
                    WeaponId = weaponId,
                    PatternStyle = "float_gun"
                };
                seed.Skins.Add(skin);
                await seed.SaveChangesAsync();
                skinId = skin.Id;

                // никаких pattern-таблиц для этого skinId добавлять не будем
            }

            var dto = new UpdateSkinDto
            {
                WeaponId = weaponId,
                Name = "  New Style Skin  ",
                PatternStyle = "ch_gun"
            };

            var result = await service.UpdateSkinAsync(skinId, dto);

            result.Should().NotBeNull();
            result!.Name.Should().Be("New Style Skin");
            result.PatternStyle.Should().Be("ch_gun");

            await using var db = new AppDbContext(options);
            var updated = await db.Skins.FirstOrDefaultAsync(s => s.Id == skinId);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("New Style Skin");
            updated.PatternStyle.Should().Be("ch_gun");
        }

        // ----------------- DeleteSkinAsync -----------------

        [Fact]
        public async Task DeleteSkinAsync_ReturnsFalse_When_Skin_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var result = await service.DeleteSkinAsync(123);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteSkinAsync_Removes_Skin_And_ReturnsTrue()
        {
            var (service, options) = CreateService();

            int skinId;
            await using (var seed = new AppDbContext(options))
            {
                var weapon = new Weapon { Name = "AK-47", WeaponTypeId = 1 };
                seed.Weapons.Add(weapon);

                var skin = new Skin
                {
                    Name = "To Delete",
                    Weapon = weapon,
                    WeaponId = weapon.Id,
                    PatternStyle = "float_gun"
                };
                seed.Skins.Add(skin);

                await seed.SaveChangesAsync();
                skinId = skin.Id;
            }

            var result = await service.DeleteSkinAsync(skinId);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.Skins.FirstOrDefaultAsync(s => s.Id == skinId);
            entity.Should().BeNull();
        }
    }
}
