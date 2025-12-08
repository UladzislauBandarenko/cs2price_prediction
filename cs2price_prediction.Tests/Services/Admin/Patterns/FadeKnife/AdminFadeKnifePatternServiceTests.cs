using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.Patterns.FadeKnife;
using cs2price_prediction.Services.Admin.Patterns.FadeKnife;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// alias на доменную сущность FadeKnifePattern
using DomainFadeKnifePattern = cs2price_prediction.Domain.Patterns.FadeKnifePattern;

namespace cs2price_prediction.Tests.Services.Admin.Patterns
{
    public class AdminFadeKnifePatternServiceTests
    {
        private (AdminFadeKnifePatternService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminFadeKnifePatternService(factoryMock.Object);
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

            var dto = new CreateFadeKnifePatternDto
            {
                SkinId = 999,
                Pattern = 1,
                FadePercentage = 90,
                FadeRank = 1
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.SkinId));
            ex.And.Message.Should().Contain("Skin not found");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Skin_PatternStyle_Not_FadeKnife()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin
                {
                    Id = 1,
                    Name = "Wrong Style Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_gun"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeKnifePatternDto
            {
                SkinId = 1,
                Pattern = 10,
                FadePercentage = 95,
                FadeRank = 1
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='fade_gun', expected 'fade_knife'.");
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
                    Name = "Fade Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_knife"
                };
                seed.Skins.Add(skin);

                seed.FadeKnifePatterns.Add(new DomainFadeKnifePattern
                {
                    SkinId = 1,
                    Pattern = 777,
                    FadePercentage = 90,
                    FadeRank = 1
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeKnifePatternDto
            {
                SkinId = 1,
                Pattern = 777,
                FadePercentage = 88,
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
                    Name = "Fade Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_knife"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateFadeKnifePatternDto
            {
                SkinId = 1,
                Pattern = 999,
                FadePercentage = 87,
                FadeRank = 3
            };

            int id = await service.CreateAsync(dto);

            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var entity = await db.FadeKnifePatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.SkinId.Should().Be(1);
            entity.Pattern.Should().Be(999);
            entity.FadePercentage.Should().Be(87);
            entity.FadeRank.Should().Be(3);
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

            var dto = new UpdateFadeKnifePatternDto
            {
                FadePercentage = 95,
                FadeRank = 1
            };

            bool result = await service.UpdateAsync(999, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Skin_PatternStyle_Not_FadeKnife()
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
                    PatternStyle = "doppler_knife"
                };
                seed.Skins.Add(skin);

                var pattern = new DomainFadeKnifePattern
                {
                    SkinId = 1,
                    Pattern = 100,
                    FadePercentage = 70,
                    FadeRank = 2,
                    Skin = skin
                };
                seed.FadeKnifePatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateFadeKnifePatternDto
            {
                FadePercentage = 99,
                FadeRank = 1
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='doppler_knife', expected 'fade_knife'.");
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
                    Name = "Fade Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_knife"
                };
                seed.Skins.Add(skin);

                var pattern = new DomainFadeKnifePattern
                {
                    SkinId = 1,
                    Pattern = 50,
                    FadePercentage = 75,
                    FadeRank = 4,
                    Skin = skin
                };
                seed.FadeKnifePatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateFadeKnifePatternDto
            {
                FadePercentage = 92,
                FadeRank = 1
            };

            bool result = await service.UpdateAsync(id, dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.FadeKnifePatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            entity.Should().NotBeNull();
            entity!.FadePercentage.Should().Be(92);
            entity.FadeRank.Should().Be(1);
            entity.Skin.PatternStyle.Should().Be("fade_knife");
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
                    Name = "Fade Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "fade_knife"
                };
                seed.Skins.Add(skin);

                var pattern = new DomainFadeKnifePattern
                {
                    SkinId = 1,
                    Pattern = 42,
                    FadePercentage = 80,
                    FadeRank = 2
                };
                seed.FadeKnifePatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            bool result = await service.DeleteAsync(id);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.FadeKnifePatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().BeNull();
        }
    }
}
