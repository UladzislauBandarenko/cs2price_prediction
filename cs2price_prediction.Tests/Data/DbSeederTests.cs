using System;
using System.IO;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.Domain.Stickers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Data
{
    public class DbSeederTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new AppDbContext(options);
        }

        private static string CreateTempDataDir()
        {
            var root = Path.Combine(Path.GetTempPath(), "cs2price_seeder_tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, "data_for_db"));
            return root;
        }

        private static void WriteFile(string root, string fileName, string content)
        {
            var path = Path.Combine(root, "data_for_db", fileName);
            File.WriteAllText(path, content);
        }

        private DbSeeder CreateSeeder(AppDbContext db, string contentRootPath)
        {
            var loggerMock = new Mock<ILogger<DbSeeder>>();
            var envMock = new Mock<IHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(contentRootPath);

            return new DbSeeder(db, loggerMock.Object, envMock.Object);
        }

        [Fact]
        public async Task SeedAsync_Populates_All_Core_Tables_From_Csv()
        {
            // arrange
            using var db = CreateDbContext(Guid.NewGuid().ToString());
            var root = CreateTempDataDir();

            // weapon_types.csv: code,name
            WriteFile(root, "weapon_types.csv",
@"code,name
rifle,Rifle
knife,Knife");

            // weapons.csv: name,weapon_type_code
            WriteFile(root, "weapons.csv",
@"name,weapon_type_code
AK-47,rifle
Bayonet,knife");

            // wear_tiers.csv: name
            WriteFile(root, "wear_tiers.csv",
@"name
Factory New
Field-Tested");

            // skins.csv: weapon_name,skin_name,pattern_style
            WriteFile(root, "skins.csv",
@"weapon_name,skin_name,pattern_style
AK-47,Case Hardened,ch_gun
Bayonet,Case Hardened,ch_knife
AK-47,Fade,fade_gun
Bayonet,Fade,fade_knife
Bayonet,Doppler,doppler_knife");

            // skin_wear_tiers.csv: weapon_name,skin_name,wear_name
            WriteFile(root, "skin_wear_tiers.csv",
@"weapon_name,skin_name,wear_name
AK-47,Case Hardened,Factory New
Bayonet,Doppler,Field-Tested");

            // case_hardened_gun_unique_patterns.csv:
            // weapon_name,skin_name,pattern,backside_blue,playside_blue
            WriteFile(root, "case_hardened_gun_unique_patterns.csv",
@"weapon_name,skin_name,pattern,backside_blue,playside_blue
AK-47,Case Hardened,777,10.5,20.5");

            // case_hardened_knives_unique_patterns.csv:
            // weapon_name,skin_name,pattern,backside_blue,backside_purple,backside_gold,playside_blue,playside_purple,playside_gold
            WriteFile(root, "case_hardened_knives_unique_patterns.csv",
@"weapon_name,skin_name,pattern,bb,bp,bg,pb,pp,pg
Bayonet,Case Hardened,888,10,20,30,40,50,60");

            // fade_gun_unique_patterns.csv:
            // weapon_name,skin_name,pattern,fade_percentage,fade_rank
            WriteFile(root, "fade_gun_unique_patterns.csv",
@"weapon_name,skin_name,pattern,fade_percentage,fade_rank
AK-47,Fade,1,95.5,1");

            // fade_knives_unique_patterns.csv:
            // weapon_name,skin_name,pattern,fade_percentage,fade_rank
            WriteFile(root, "fade_knives_unique_patterns.csv",
@"weapon_name,skin_name,pattern,fade_percentage,fade_rank
Bayonet,Fade,2,90.0,2");

            // doppler_phases.csv: name
            WriteFile(root, "doppler_phases.csv",
@"name
Ruby");

            // doppler_skin_phases.csv: weapon_name,skin_name,phase_name
            WriteFile(root, "doppler_skin_phases.csv",
@"weapon_name,skin_name,phase_name
Bayonet,Doppler,Ruby");

            // stickers_dataset.csv: sticker_id,name,reference_price
            WriteFile(root, "stickers_dataset.csv",
@"sticker_id,name,reference_price
1,Sticker One,1.23
2,Sticker Two,2.50");

            var seeder = CreateSeeder(db, root);

            // act
            await seeder.SeedAsync();

            // assert basic counts
            (await db.WeaponTypes.CountAsync()).Should().Be(2);
            (await db.Weapons.CountAsync()).Should().Be(2);
            (await db.WearTiers.CountAsync()).Should().Be(2);
            (await db.Skins.CountAsync()).Should().Be(5);
            (await db.SkinWearTiers.CountAsync()).Should().Be(2);

            (await db.CaseHardenedGunPatterns.CountAsync()).Should().Be(1);
            (await db.CaseHardenedKnifePatterns.CountAsync()).Should().Be(1);
            (await db.FadeGunPatterns.CountAsync()).Should().Be(1);
            (await db.FadeKnifePatterns.CountAsync()).Should().Be(1);

            (await db.DopplerPhases.CountAsync()).Should().Be(1);
            (await db.DopplerSkinPhases.CountAsync()).Should().Be(1);

            (await db.Stickers.CountAsync()).Should().Be(2);
            (await db.StickerPrices.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task SeedAsync_Second_Run_Does_Not_Duplicate_Data()
        {
            using var db = CreateDbContext(Guid.NewGuid().ToString());
            var root = CreateTempDataDir();

            // минимальный набор файлов, аналогичный первому тесту, но можно урезать
            WriteFile(root, "weapon_types.csv",
@"code,name
rifle,Rifle");

            WriteFile(root, "weapons.csv",
@"name,weapon_type_code
AK-47,rifle");

            WriteFile(root, "wear_tiers.csv",
@"name
Factory New");

            WriteFile(root, "skins.csv",
@"weapon_name,skin_name,pattern_style
AK-47,Case Hardened,ch_gun");

            WriteFile(root, "skin_wear_tiers.csv",
@"weapon_name,skin_name,wear_name
AK-47,Case Hardened,Factory New");

            WriteFile(root, "stickers_dataset.csv",
@"sticker_id,name,reference_price
1,Sticker One,1.23");

            var seeder = CreateSeeder(db, root);

            // первый запуск
            await seeder.SeedAsync();

            var wtCount1 = await db.WeaponTypes.CountAsync();
            var wCount1 = await db.Weapons.CountAsync();
            var wearCount1 = await db.WearTiers.CountAsync();
            var skinCount1 = await db.Skins.CountAsync();
            var swtCount1 = await db.SkinWearTiers.CountAsync();
            var stickerCount1 = await db.Stickers.CountAsync();
            var priceCount1 = await db.StickerPrices.CountAsync();

            // второй запуск (все AnyAsync должны сработать и вернуть, ничего не добавляя)
            await seeder.SeedAsync();

            (await db.WeaponTypes.CountAsync()).Should().Be(wtCount1);
            (await db.Weapons.CountAsync()).Should().Be(wCount1);
            (await db.WearTiers.CountAsync()).Should().Be(wearCount1);
            (await db.Skins.CountAsync()).Should().Be(skinCount1);
            (await db.SkinWearTiers.CountAsync()).Should().Be(swtCount1);
            (await db.Stickers.CountAsync()).Should().Be(stickerCount1);
            (await db.StickerPrices.CountAsync()).Should().Be(priceCount1);
        }
    }
}
