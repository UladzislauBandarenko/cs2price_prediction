using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Services.Meta;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Domain namespaces
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.Domain.Stickers;

namespace cs2price_prediction.Tests.Services.Meta
{
    public class MetaServiceTests
    {
        private AppDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new AppDbContext(options);
        }

        private MetaService CreateService(AppDbContext db) => new MetaService(db);

        // ----------------------------------------------------------------------
        // GetWeaponTypesAsync
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetWeaponTypesAsync_Returns_Sorted()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.WeaponTypes.AddRange(
                new WeaponType { Id = 3, Code = "smg", Name = "SMG" },
                new WeaponType { Id = 1, Code = "rifle", Name = "Rifle" },
                new WeaponType { Id = 2, Code = "pistol", Name = "Pistol" }
            );
            await db.SaveChangesAsync();

            var service = CreateService(db);

            var result = (await service.GetWeaponTypesAsync()).ToList();

            result.Select(x => x.Id).Should().Equal(1, 2, 3);
        }

        // ----------------------------------------------------------------------
        // GetWeaponsForTypeAsync
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetWeaponsForTypeAsync_Filters_And_Sorts()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Weapons.AddRange(
                new Weapon { Id = 1, Name = "AK-47", WeaponTypeId = 1 },
                new Weapon { Id = 3, Name = "Glock-18", WeaponTypeId = 2 },
                new Weapon { Id = 2, Name = "M4A4", WeaponTypeId = 1 }
            );
            await db.SaveChangesAsync();

            var service = CreateService(db);

            var result = (await service.GetWeaponsForTypeAsync(1)).ToList();

            result.Should().HaveCount(2);
            result.Select(x => x.Name).Should().Equal("AK-47", "M4A4");
        }

        // ----------------------------------------------------------------------
        // GetSkinsForWeaponAsync
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetSkinsForWeaponAsync_Filters_And_Sorts()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Skins.AddRange(
                new Skin { Id = 1, Name = "Redline", WeaponId = 1, PatternStyle = "float_gun" },
                new Skin { Id = 2, Name = "Blue Laminate", WeaponId = 1, PatternStyle = "ch_gun" },
                new Skin { Id = 3, Name = "Asiimov", WeaponId = 2, PatternStyle = "fade_gun" }
            );
            await db.SaveChangesAsync();

            var service = CreateService(db);

            var result = (await service.GetSkinsForWeaponAsync(1)).ToList();

            result.Select(x => x.Name).Should().Equal("Blue Laminate", "Redline");
        }

        // ----------------------------------------------------------------------
        // GetWearForSkinAsync
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetWearForSkinAsync_Returns_All_Wears()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            var w1 = new WearTier { Id = 1, Name = "Factory New" };
            var w2 = new WearTier { Id = 2, Name = "Field-Tested" };
            var w3 = new WearTier { Id = 3, Name = "Minimal Wear" };

            db.WearTiers.AddRange(w1, w2, w3);
            db.Skins.Add(new Skin { Id = 1, Name = "TestSkin", WeaponId = 1, PatternStyle = "float_gun" });

            db.SkinWearTiers.AddRange(
                new SkinWearTier { SkinId = 1, WearTierId = 3, WearTier = w3 },
                new SkinWearTier { SkinId = 1, WearTierId = 2, WearTier = w2 }
            );

            await db.SaveChangesAsync();

            var service = CreateService(db);

            var result = (await service.GetWearForSkinAsync(1)).ToList();

            result.Select(x => x.Name).Should().Equal("Field-Tested", "Minimal Wear");
        }

        // ----------------------------------------------------------------------
        // GetPatternsForSkinAsync — skin not found
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetPatternsForSkinAsync_SkinNotFound()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());
            var service = CreateService(db);

            var (exists, patterns) = await service.GetPatternsForSkinAsync(999);

            exists.Should().BeFalse();
            patterns.Should().BeEmpty();
        }

        // ----------------------------------------------------------------------
        // ch_gun patterns
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetPatternsForSkinAsync_CaseHardenedGuns()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "CH Test",
                WeaponId = 1,
                PatternStyle = "ch_gun"
            });

            db.CaseHardenedGunPatterns.AddRange(
                new CaseHardenedGunPattern { Id = 1, SkinId = 1, Pattern = 10 },
                new CaseHardenedGunPattern { Id = 2, SkinId = 1, Pattern = 5 }
            );

            await db.SaveChangesAsync();

            var service = CreateService(db);

            var (exists, patterns) = await service.GetPatternsForSkinAsync(1);

            exists.Should().BeTrue();
            patterns.Select(p => p.Id).Should().Equal(5, 10);
        }

        // ----------------------------------------------------------------------
        // ch_knife patterns
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetPatternsForSkinAsync_CaseHardenedKnife()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Skins.Add(new Skin
            {
                Id = 1,
                PatternStyle = "ch_knife",
                WeaponId = 1,
                Name = "Test Knife"
            });

            db.CaseHardenedKnifePatterns.AddRange(
                new CaseHardenedKnifePattern { Id = 1, SkinId = 1, Pattern = 100 },
                new CaseHardenedKnifePattern { Id = 2, SkinId = 1, Pattern = 50 }
            );

            await db.SaveChangesAsync();

            var service = CreateService(db);

            var (exists, patterns) = await service.GetPatternsForSkinAsync(1);

            exists.Should().BeTrue();
            patterns.Select(p => p.Id).Should().Equal(50, 100);
        }

        // ----------------------------------------------------------------------
        // fade_gun patterns
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetPatternsForSkinAsync_FadeGun()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Skins.Add(new Skin
            {
                Id = 1,
                PatternStyle = "fade_gun",
                WeaponId = 1,
                Name = "Test Fade Gun"
            });

            db.FadeGunPatterns.AddRange(
                new FadeGunPattern { Id = 1, SkinId = 1, Pattern = 2 },
                new FadeGunPattern { Id = 2, SkinId = 1, Pattern = 1 }
            );

            await db.SaveChangesAsync();

            var service = CreateService(db);

            var (exists, patterns) = await service.GetPatternsForSkinAsync(1);

            exists.Should().BeTrue();
            patterns.Select(p => p.Id).Should().Equal(1, 2);
        }

        // ----------------------------------------------------------------------
        // fade_knife patterns
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetPatternsForSkinAsync_FadeKnife()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Skins.Add(new Skin
            {
                Id = 1,
                PatternStyle = "fade_knife",
                WeaponId = 1,
                Name = "Test Fade Knife"
            });

            db.FadeKnifePatterns.AddRange(
                new FadeKnifePattern { Id = 1, SkinId = 1, Pattern = 3 },
                new FadeKnifePattern { Id = 2, SkinId = 1, Pattern = 1 }
            );

            await db.SaveChangesAsync();

            var service = CreateService(db);

            var (exists, patterns) = await service.GetPatternsForSkinAsync(1);

            exists.Should().BeTrue();
            patterns.Select(p => p.Id).Should().Equal(1, 3);
        }

        // ----------------------------------------------------------------------
        // doppler_knife patterns (PhaseId + Phase.Name)
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetPatternsForSkinAsync_DopplerKnife()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            var p1 = new DopplerPhase { Id = 1, Name = "Phase 1" };
            var p2 = new DopplerPhase { Id = 2, Name = "Phase 2" };

            db.DopplerPhases.AddRange(p1, p2);

            db.Skins.Add(new Skin
            {
                Id = 1,
                PatternStyle = "doppler_knife",
                WeaponId = 1,
                Name = "Doppler"
            });

            db.DopplerSkinPhases.AddRange(
                new DopplerSkinPhase { Id = 1, SkinId = 1, PhaseId = 2, Phase = p2 },
                new DopplerSkinPhase { Id = 2, SkinId = 1, PhaseId = 1, Phase = p1 }
            );

            await db.SaveChangesAsync();

            var service = CreateService(db);

            var (exists, patterns) = await service.GetPatternsForSkinAsync(1);

            exists.Should().BeTrue();

            var list = patterns.ToList();
            list[0].Id.Should().Be(1);     // PhaseId
            list[0].Name.Should().Be("Phase 1");
            list[1].Id.Should().Be(2);
            list[1].Name.Should().Be("Phase 2");
        }

        // ----------------------------------------------------------------------
        // GetStickersAsync — filtering test
        // ----------------------------------------------------------------------

        [Fact]
        public async Task GetStickersAsync_Filters_By_Name()
        {
            using var db = CreateDb(Guid.NewGuid().ToString());

            db.Stickers.AddRange(
                new Sticker { Id = 1, Name = "Navi Holo" },
                new Sticker { Id = 2, Name = "FaZe Holo" },
                new Sticker { Id = 3, Name = "Simple Gold" }
            );
            await db.SaveChangesAsync();

            var service = CreateService(db);

            // EF InMemory НЕ МОЖЕТ выполнить ILIKE → фильтруем вручную
            var result = (await service.GetStickersAsync("holo", 50)).ToList();

            // Эмулируем то, что сделал бы PostgreSQL
            var expected = new[] { "Navi Holo", "FaZe Holo" };

            result.Should().NotBeNull();
            result.Select(x => x.Name).Should().BeEquivalentTo(expected);
        }

    }
}
