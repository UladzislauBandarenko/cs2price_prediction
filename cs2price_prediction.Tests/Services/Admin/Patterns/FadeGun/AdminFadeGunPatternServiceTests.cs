using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.Patterns.FadeGun;
using cs2price_prediction.Services.Admin.Patterns.FadeGun;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// alias на доменную сущность FadeGunPattern
using DomainFadeGunPattern = cs2price_prediction.Domain.Patterns.FadeGunPattern;

namespace cs2price_prediction.Tests.Services.Admin.Patterns
{
    public class AdminFadeGunPatternServiceTests
    {
        private (AdminFadeGunPatternService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminFadeGunPatternService(factoryMock.Object);
            return (service, options);
        }

        // ----------------- CreateAsync -----------------

        [Fact]
        public async Task CreateAsync_Throws_When_Skin_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeGunPatternDto
            {
                SkinId = 999,
                Pattern = 1,
                FadePercentage = 80,
                FadeRank = 1
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.SkinId));
            ex.And.Message.Should().Contain("Skin not found");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Skin_PatternStyle_Not_FadeGun()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin
                {
                    Id = 1,
                    Name = "Wrong Style Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_gun"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeGunPatternDto
            {
                SkinId = 1,
                Pattern = 100,
                FadePercentage = 90,
                FadeRank = 1
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='ch_gun', expected 'fade_gun'.");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Pattern_Already_Exists()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Fade Gun Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_gun"
                };
                seed.Skins.Add(skin);

                seed.FadeGunPatterns.Add(new DomainFadeGunPattern
                {
                    SkinId = 1,
                    Pattern = 777,
                    FadePercentage = 95,
                    FadeRank = 1
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeGunPatternDto
            {
                SkinId = 1,
                Pattern = 777,
                FadePercentage = 80,
                FadeRank = 2
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Pattern already exists for this skin and pattern number.");
        }

        [Fact]
        public async Task CreateAsync_Creates_Pattern_When_All_Ok()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin
                {
                    Id = 1,
                    Name = "Fade Gun Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_gun"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeGunPatternDto
            {
                SkinId = 1,
                Pattern = 999,
                FadePercentage = 88,
                FadeRank = 2
            };

            int id = await service.CreateAsync(dto);

            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var entity = await db.FadeGunPatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.SkinId.Should().Be(1);
            entity.Pattern.Should().Be(999);
            entity.FadePercentage.Should().Be(88);
            entity.FadeRank.Should().Be(2);
        }

        // ----------------- UpdateAsync -----------------

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_When_Entity_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var dto = new UpdateFadeGunPatternDto
            {
                FadePercentage = 90,
                FadeRank = 1
            };

            bool result = await service.UpdateAsync(999, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Skin_PatternStyle_Not_FadeGun()
        {
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Wrong Style Skin",
                    WeaponId = 1,
                    PatternStyle = "float_gun"
                };
                seed.Skins.Add(skin);

                var pattern = new DomainFadeGunPattern
                {
                    SkinId = 1,
                    Pattern = 100,
                    FadePercentage = 70,
                    FadeRank = 1,
                    Skin = skin
                };
                seed.FadeGunPatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateFadeGunPatternDto
            {
                FadePercentage = 95,
                FadeRank = 1
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='float_gun', expected 'fade_gun'.");
        }

        [Fact]
        public async Task UpdateAsync_Updates_When_All_Ok()
        {
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Fade Gun Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_gun"
                };
                seed.Skins.Add(skin);

                var pattern = new DomainFadeGunPattern
                {
                    SkinId = 1,
                    Pattern = 123,
                    FadePercentage = 70,
                    FadeRank = 3,
                    Skin = skin
                };
                seed.FadeGunPatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateFadeGunPatternDto
            {
                FadePercentage = 99,
                FadeRank = 1
            };

            bool result = await service.UpdateAsync(id, dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.FadeGunPatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            entity.Should().NotBeNull();
            entity!.FadePercentage.Should().Be(99);
            entity.FadeRank.Should().Be(1);
            entity.Skin.PatternStyle.Should().Be("fade_gun");
        }

        // ----------------- DeleteAsync -----------------

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_When_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            bool result = await service.DeleteAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Removes_Entity_And_ReturnsTrue()
        {
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Fade Gun Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_gun"
                };
                seed.Skins.Add(skin);

                var pattern = new DomainFadeGunPattern
                {
                    SkinId = 1,
                    Pattern = 42,
                    FadePercentage = 80,
                    FadeRank = 2
                };
                seed.FadeGunPatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            bool result = await service.DeleteAsync(id);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.FadeGunPatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().BeNull();
        }
    }
}
