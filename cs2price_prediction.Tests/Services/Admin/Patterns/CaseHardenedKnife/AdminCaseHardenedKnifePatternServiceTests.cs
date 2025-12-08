using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedKnife;
using cs2price_prediction.Services.Admin.Patterns.CaseHardenedKnife;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.Patterns.CaseHardenedKnife
{
    public class AdminCaseHardenedKnifePatternServiceTests
    {
        private (AdminCaseHardenedKnifePatternService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminCaseHardenedKnifePatternService(factoryMock.Object);
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

            var dto = new CreateCaseHardenedKnifePatternDto
            {
                SkinId = 999,
                Pattern = 1,
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.SkinId));
            ex.And.Message.Should().Contain("Skin not found");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Skin_PatternStyle_Not_ChKnife()
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

            var dto = new CreateCaseHardenedKnifePatternDto
            {
                SkinId = 1,
                Pattern = 100,
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='ch_gun', expected 'ch_knife'.");
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
                    Name = "CH Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_knife"
                };
                seed.Skins.Add(skin);

                seed.CaseHardenedKnifePatterns.Add(new CaseHardenedKnifePattern
                {
                    Id = 1,
                    SkinId = 1,
                    Pattern = 777,
                    BacksideBlue = 1,
                    BacksidePurple = 2,
                    BacksideGold = 3,
                    PlaysideBlue = 4,
                    PlaysidePurple = 5,
                    PlaysideGold = 6
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateCaseHardenedKnifePatternDto
            {
                SkinId = 1,
                Pattern = 777,
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60
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
                    Name = "CH Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_knife"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateCaseHardenedKnifePatternDto
            {
                SkinId = 1,
                Pattern = 999,
                BacksideBlue = 11,
                BacksidePurple = 22,
                BacksideGold = 33,
                PlaysideBlue = 44,
                PlaysidePurple = 55,
                PlaysideGold = 66
            };

            int id = await service.CreateAsync(dto);

            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var entity = await db.CaseHardenedKnifePatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.SkinId.Should().Be(1);
            entity.Pattern.Should().Be(999);
            entity.BacksideBlue.Should().Be(11);
            entity.BacksidePurple.Should().Be(22);
            entity.BacksideGold.Should().Be(33);
            entity.PlaysideBlue.Should().Be(44);
            entity.PlaysidePurple.Should().Be(55);
            entity.PlaysideGold.Should().Be(66);
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

            var dto = new UpdateCaseHardenedKnifePatternDto
            {
                BacksideBlue = 1,
                BacksidePurple = 2,
                BacksideGold = 3,
                PlaysideBlue = 4,
                PlaysidePurple = 5,
                PlaysideGold = 6
            };

            bool result = await service.UpdateAsync(123, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Skin_PatternStyle_Not_ChKnife()
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

                var pattern = new CaseHardenedKnifePattern
                {
                    SkinId = 1,
                    Pattern = 100,
                    BacksideBlue = 1,
                    BacksidePurple = 2,
                    BacksideGold = 3,
                    PlaysideBlue = 4,
                    PlaysidePurple = 5,
                    PlaysideGold = 6,
                    Skin = skin
                };
                seed.CaseHardenedKnifePatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateCaseHardenedKnifePatternDto
            {
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='doppler_knife', expected 'ch_knife'.");
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
                    Name = "CH Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_knife"
                };
                seed.Skins.Add(skin);

                var pattern = new CaseHardenedKnifePattern
                {
                    SkinId = 1,
                    Pattern = 100,
                    BacksideBlue = 1,
                    BacksidePurple = 2,
                    BacksideGold = 3,
                    PlaysideBlue = 4,
                    PlaysidePurple = 5,
                    PlaysideGold = 6,
                    Skin = skin
                };
                seed.CaseHardenedKnifePatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateCaseHardenedKnifePatternDto
            {
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60
            };

            bool result = await service.UpdateAsync(id, dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.CaseHardenedKnifePatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            entity.Should().NotBeNull();
            entity!.BacksideBlue.Should().Be(10);
            entity.BacksidePurple.Should().Be(20);
            entity.BacksideGold.Should().Be(30);
            entity.PlaysideBlue.Should().Be(40);
            entity.PlaysidePurple.Should().Be(50);
            entity.PlaysideGold.Should().Be(60);
            entity.Skin.PatternStyle.Should().Be("ch_knife");
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
                    Name = "CH Knife Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_knife"
                };
                seed.Skins.Add(skin);

                var pattern = new CaseHardenedKnifePattern
                {
                    SkinId = 1,
                    Pattern = 500,
                    BacksideBlue = 1,
                    BacksidePurple = 2,
                    BacksideGold = 3,
                    PlaysideBlue = 4,
                    PlaysidePurple = 5,
                    PlaysideGold = 6
                };
                seed.CaseHardenedKnifePatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            bool result = await service.DeleteAsync(id);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.CaseHardenedKnifePatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().BeNull();
        }
    }
}
