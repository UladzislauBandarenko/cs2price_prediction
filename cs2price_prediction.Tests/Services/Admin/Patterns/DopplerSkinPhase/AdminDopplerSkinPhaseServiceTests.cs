using System;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.DTOs.Admin.Patterns.DopplerSkin;
using cs2price_prediction.Services.Admin.Patterns.DopplerSkinPhase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

// alias на доменные сущности
using DomainDopplerSkinPhase = cs2price_prediction.Domain.Patterns.DopplerSkinPhase;
using DomainDopplerPhase = cs2price_prediction.Domain.Patterns.DopplerPhase;

namespace cs2price_prediction.Tests.Services.Admin.Patterns
{
    public class AdminDopplerSkinPhaseServiceTests
    {
        private (AdminDopplerSkinPhaseService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminDopplerSkinPhaseService(factoryMock.Object);
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

            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 999,
                PhaseId = 1
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.SkinId));
            ex.And.Message.Should().Contain("Skin not found");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Skin_PatternStyle_Not_DopplerKnife()
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
                seed.DopplerPhases.Add(new DomainDopplerPhase
                {
                    Id = 1,
                    Name = "Phase 1"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 1,
                PhaseId = 1
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='float_gun', expected 'doppler_knife'.");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Phase_Not_Found()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin
                {
                    Id = 1,
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                });
                await seed.SaveChangesAsync();
            }

            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 1,
                PhaseId = 999
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.PhaseId));
            ex.And.Message.Should().Contain("Phase not found");
        }

        [Fact]
        public async Task CreateAsync_Throws_When_Duplicate_Skin_Phase()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                };
                seed.Skins.Add(skin);

                var phase = new DomainDopplerPhase
                {
                    Id = 2,
                    Name = "Phase 2"
                };
                seed.DopplerPhases.Add(phase);

                seed.DopplerSkinPhases.Add(new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 2
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 1,
                PhaseId = 2
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This skin already has this doppler phase.");
        }

        [Fact]
        public async Task CreateAsync_Creates_When_All_Ok()
        {
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                seed.Skins.Add(new Skin
                {
                    Id = 1,
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                });

                seed.DopplerPhases.Add(new DomainDopplerPhase
                {
                    Id = 3,
                    Name = "Phase 3"
                });

                await seed.SaveChangesAsync();
            }

            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 1,
                PhaseId = 3
            };

            int id = await service.CreateAsync(dto);

            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var entity = await db.DopplerSkinPhases.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().NotBeNull();
            entity!.SkinId.Should().Be(1);
            entity.PhaseId.Should().Be(3);
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

            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 1
            };

            bool result = await service.UpdateAsync(999, dto);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Skin_PatternStyle_Not_DopplerKnife()
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
                    PatternStyle = "ch_knife"
                };
                seed.Skins.Add(skin);

                seed.DopplerPhases.Add(new DomainDopplerPhase
                {
                    Id = 1,
                    Name = "Phase 1"
                });

                var link = new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 1,
                    Skin = skin
                };
                seed.DopplerSkinPhases.Add(link);

                await seed.SaveChangesAsync();
                id = link.Id;
            }

            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 1
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Skin 1 has patternStyle='ch_knife', expected 'doppler_knife'.");
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Phase_Not_Found()
        {
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                };
                seed.Skins.Add(skin);

                seed.DopplerPhases.Add(new DomainDopplerPhase
                {
                    Id = 1,
                    Name = "Phase 1"
                });

                var link = new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 1,
                    Skin = skin
                };
                seed.DopplerSkinPhases.Add(link);

                await seed.SaveChangesAsync();
                id = link.Id;
            }

            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 999
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.And.ParamName.Should().Be(nameof(dto.PhaseId));
            ex.And.Message.Should().Contain("Phase not found");
        }

        [Fact]
        public async Task UpdateAsync_Throws_When_Duplicate_Skin_Phase()
        {
            var (service, options) = CreateService();

            int id;
            await using (var seed = new AppDbContext(options))
            {
                var skin = new Skin
                {
                    Id = 1,
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                };
                seed.Skins.Add(skin);

                seed.DopplerPhases.AddRange(
                    new DomainDopplerPhase { Id = 1, Name = "Phase 1" },
                    new DomainDopplerPhase { Id = 2, Name = "Phase 2" }
                );

                var link1 = new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 1,
                    Skin = skin
                };
                var link2 = new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 2
                };

                seed.DopplerSkinPhases.AddRange(link1, link2);
                await seed.SaveChangesAsync();
                id = link1.Id;
            }

            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 2
            };

            Func<Task> act = async () => await service.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This skin already has this doppler phase.");
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
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                };
                seed.Skins.Add(skin);

                seed.DopplerPhases.AddRange(
                    new DomainDopplerPhase { Id = 1, Name = "Phase 1" },
                    new DomainDopplerPhase { Id = 2, Name = "Phase 2" }
                );

                var link = new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 1,
                    Skin = skin
                };
                seed.DopplerSkinPhases.Add(link);

                await seed.SaveChangesAsync();
                id = link.Id;
            }

            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 2
            };

            bool result = await service.UpdateAsync(id, dto);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var updated = await db.DopplerSkinPhases.FirstOrDefaultAsync(p => p.Id == id);
            updated.Should().NotBeNull();
            updated!.PhaseId.Should().Be(2);
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
                    Name = "Doppler Skin",
                    WeaponId = 1,
                    PatternStyle = "doppler_knife"
                };
                seed.Skins.Add(skin);

                seed.DopplerPhases.Add(new DomainDopplerPhase
                {
                    Id = 1,
                    Name = "Phase 1"
                });

                var link = new DomainDopplerSkinPhase
                {
                    SkinId = 1,
                    PhaseId = 1
                };
                seed.DopplerSkinPhases.Add(link);

                await seed.SaveChangesAsync();
                id = link.Id;
            }

            bool result = await service.DeleteAsync(id);

            result.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var entity = await db.DopplerSkinPhases.FirstOrDefaultAsync(p => p.Id == id);
            entity.Should().BeNull();
        }
    }
}
