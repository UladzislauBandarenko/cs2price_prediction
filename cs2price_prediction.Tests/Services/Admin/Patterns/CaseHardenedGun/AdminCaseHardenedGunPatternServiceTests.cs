using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedGun;
using cs2price_prediction.Services.Admin.Patterns.CaseHardenedGun;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.Patterns.CaseHardenedGun
{
    public class AdminCaseHardenedGunPatternServiceTests
    {
        private (AdminCaseHardenedGunPatternService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminCaseHardenedGunPatternService(factoryMock.Object);
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

            var dto = new CreateCaseHardenedGunPatternDto
            {
                SkinId = 999,
                Pattern = 1,
                PlaysideBlue = 10,
                BacksideBlue = 20
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.SkinId));
            ex.And.Message.Should().Contain("Skin not found");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Skin_PatternStyle_Not_ChGun()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin
                {
                    Id = 1,
                    Name = "Wrong Style Skin",
                    WeaponId = 1,
                    PatternStyle = "float_gun"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateCaseHardenedGunPatternDto
            {
                SkinId = 1,
                Pattern = 100,
                PlaysideBlue = 50,
                BacksideBlue = 60
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='float_gun', expected 'ch_gun'.");
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
                    Name = "CH Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_gun"
                };
                seed.Skins.Add(skin);

                seed.CaseHardenedGunPatterns.Add(new CaseHardenedGunPattern
                {
                    Id = 1,
                    SkinId = 1,
                    Pattern = 123,
                    PlaysideBlue = 10,
                    BacksideBlue = 20
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateCaseHardenedGunPatternDto
            {
                SkinId = 1,
                Pattern = 123,
                PlaysideBlue = 99,
                BacksideBlue = 99
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
                    Name = "CH Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_gun"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateCaseHardenedGunPatternDto
            {
                SkinId = 1,
                Pattern = 777,
                PlaysideBlue = 80,
                BacksideBlue = 70
            };

            int id = await service.CreateAsync(dto);

            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var entity = await db.CaseHardenedGunPatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.SkinId.Should().Be(1);
            entity.Pattern.Should().Be(777);
            entity.PlaysideBlue.Should().Be(80);
            entity.BacksideBlue.Should().Be(70);
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

            var dto = new UpdateCaseHardenedGunPatternDto
            {
                PlaysideBlue = 10,
                BacksideBlue = 20
            };

            bool result = await service.UpdateAsync(999, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Skin_PatternStyle_Not_ChGun()
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

                var pattern = new CaseHardenedGunPattern
                {
                    SkinId = 1,
                    Pattern = 100,
                    PlaysideBlue = 10,
                    BacksideBlue = 20,
                    Skin = skin
                };
                seed.CaseHardenedGunPatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateCaseHardenedGunPatternDto
            {
                PlaysideBlue = 50,
                BacksideBlue = 60
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='doppler_knife', expected 'ch_gun'.");
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
                    Name = "CH Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_gun"
                };
                seed.Skins.Add(skin);

                var pattern = new CaseHardenedGunPattern
                {
                    SkinId = 1,
                    Pattern = 100,
                    PlaysideBlue = 10,
                    BacksideBlue = 20,
                    Skin = skin
                };
                seed.CaseHardenedGunPatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            var dto = new UpdateCaseHardenedGunPatternDto
            {
                PlaysideBlue = 90,
                BacksideBlue = 80
            };

            bool result = await service.UpdateAsync(id, dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.CaseHardenedGunPatterns
                .Include(p => p.Skin)
                .FirstOrDefaultAsync(p => p.Id == id);

            entity.Should().NotBeNull();
            entity!.PlaysideBlue.Should().Be(90);
            entity.BacksideBlue.Should().Be(80);
            entity.Skin.PatternStyle.Should().Be("ch_gun");
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

            bool result = await service.DeleteAsync(123);

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
                    Name = "CH Skin",
                    WeaponId = 1,
                    PatternStyle = "ch_gun"
                };
                seed.Skins.Add(skin);

                var pattern = new CaseHardenedGunPattern
                {
                    SkinId = 1,
                    Pattern = 200,
                    PlaysideBlue = 10,
                    BacksideBlue = 20
                };
                seed.CaseHardenedGunPatterns.Add(pattern);

                await seed.SaveChangesAsync();
                id = pattern.Id;
            }

            bool result = await service.DeleteAsync(id);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.CaseHardenedGunPatterns.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().BeNull();
        }
    }
}
