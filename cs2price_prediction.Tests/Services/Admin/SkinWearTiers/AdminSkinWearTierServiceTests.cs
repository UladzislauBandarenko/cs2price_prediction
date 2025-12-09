using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.SkinWearTiers;
using cs2price_prediction.Services.Admin.SkinWearTiers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.SkinWearTiers
{
    public class AdminSkinWearTierServiceTests
    {
        private (AdminSkinWearTierService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminSkinWearTierService(factoryMock.Object);
            return (service, options);
        }

        // ----------------- CreateSkinWearTierAsync -----------------

        [Fact]
        public async Task CreateSkinWearTierAsync_ReturnsFalse_When_Already_Exists()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });
                seed.WearTiers.Add(new WearTier { Id = 2, Name = "FT" });

                seed.SkinWearTiers.Add(new SkinWearTier
                {
                    SkinId = 1,
                    WearTierId = 2
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateSkinWearTierDto
            {
                SkinId = 1,
                WearTierId = 2
            };

            var result = await service.CreateSkinWearTierAsync(dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateSkinWearTierAsync_Throws_When_Skin_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.WearTiers.Add(new WearTier { Id = 2, Name = "FT" });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateSkinWearTierDto
            {
                SkinId = 999,
                WearTierId = 2
            };

            Func<Task> act = async () => await service.CreateSkinWearTierAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.SkinId));
            ex.And.Message.Should().Contain("Skin not found");
        }

        [Fact]
        public async Task CreateSkinWearTierAsync_Throws_When_WearTier_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateSkinWearTierDto
            {
                SkinId = 1,
                WearTierId = 999
            };

            Func<Task> act = async () => await service.CreateSkinWearTierAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.WearTierId));
            ex.And.Message.Should().Contain("WearTier not found");
        }

        [Fact]
        public async Task CreateSkinWearTierAsync_Creates_When_All_Ok()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });
                seed.WearTiers.Add(new WearTier { Id = 2, Name = "FT" });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateSkinWearTierDto
            {
                SkinId = 1,
                WearTierId = 2
            };

            var result = await service.CreateSkinWearTierAsync(dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var link = await db.SkinWearTiers.FirstOrDefaultAsync(sw => sw.SkinId == 1 && sw.WearTierId == 2);
            link.Should().NotBeNull();
        }

        // ----------------- DeleteSkinWearTierAsync -----------------

        [Fact]
        public async Task DeleteSkinWearTierAsync_ReturnsFalse_When_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var result = await service.DeleteSkinWearTierAsync(1, 2);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteSkinWearTierAsync_Removes_Entity_And_ReturnsTrue()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });
                seed.WearTiers.Add(new WearTier { Id = 2, Name = "FT" });

                seed.SkinWearTiers.Add(new SkinWearTier
                {
                    SkinId = 1,
                    WearTierId = 2
                });

                await seed.SaveChangesAsync();
            }

            var result = await service.DeleteSkinWearTierAsync(1, 2);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var link = await db.SkinWearTiers.FirstOrDefaultAsync(sw => sw.SkinId == 1 && sw.WearTierId == 2);
            link.Should().BeNull();
        }

        // ----------------- UpdateSkinWearTierAsync -----------------

        [Fact]
        public async Task UpdateSkinWearTierAsync_ReturnsFalse_When_Link_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });
                seed.WearTiers.AddRange(
                    new WearTier { Id = 2, Name = "FT" },
                    new WearTier { Id = 3, Name = "MW" }
                );
                await seed.SaveChangesAsync();
            }

            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 999,
                NewWearTierId = 3
            };

            var result = await service.UpdateSkinWearTierAsync(dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateSkinWearTierAsync_Throws_When_NewWearTier_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });

                seed.WearTiers.Add(new WearTier { Id = 2, Name = "FT" });

                seed.SkinWearTiers.Add(new SkinWearTier
                {
                    SkinId = 1,
                    WearTierId = 2
                });

                await seed.SaveChangesAsync();
            }

            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 999
            };

            Func<Task> act = async () => await service.UpdateSkinWearTierAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.NewWearTierId));
            ex.And.Message.Should().Contain("New WearTier not found");
        }

        [Fact]
        public async Task UpdateSkinWearTierAsync_Throws_When_Duplicate_New_Pair()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });

                var w2 = new WearTier { Id = 2, Name = "FT" };
                var w3 = new WearTier { Id = 3, Name = "MW" };
                seed.WearTiers.AddRange(w2, w3);

                seed.SkinWearTiers.AddRange(
                    new SkinWearTier { SkinId = 1, WearTierId = 2 },
                    new SkinWearTier { SkinId = 1, WearTierId = 3 } // уже есть новая пара
                );

                await seed.SaveChangesAsync();
            }

            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 3
            };

            Func<Task> act = async () => await service.UpdateSkinWearTierAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("SkinWearTier with the new WearTier already exists.");
        }

        [Fact]
        public async Task UpdateSkinWearTierAsync_Updates_When_All_Ok()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin { Id = 1, Name = "Skin1", WeaponId = 1, PatternStyle = "float_gun" });

                var w2 = new WearTier { Id = 2, Name = "FT" };
                var w3 = new WearTier { Id = 3, Name = "MW" };
                seed.WearTiers.AddRange(w2, w3);

                seed.SkinWearTiers.Add(
                    new SkinWearTier { SkinId = 1, WearTierId = 2 }
                );

                await seed.SaveChangesAsync();
            }

            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 3
            };

            var result = await service.UpdateSkinWearTierAsync(dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var linkOld = await db.SkinWearTiers.FirstOrDefaultAsync(sw => sw.SkinId == 1 && sw.WearTierId == 2);
            var linkNew = await db.SkinWearTiers.FirstOrDefaultAsync(sw => sw.SkinId == 1 && sw.WearTierId == 3);

            linkOld.Should().BeNull();
            linkNew.Should().NotBeNull();
        }
    }
}
