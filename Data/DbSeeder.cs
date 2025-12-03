using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.Domain.Stickers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cs2price_prediction.Data
{
    public class DbSeeder
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DbSeeder> _logger;
        private readonly IHostEnvironment _env;

        public DbSeeder(AppDbContext db, ILogger<DbSeeder> logger, IHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _env = env;
        }

        private string GetDataPath(string fileName)
        {
            // ВСЕ csv лежат в cs2price_prediction/data_for_db/
            var path = Path.Combine(_env.ContentRootPath, "data_for_db", fileName);
            _logger.LogInformation("[DbSeeder] Resolving data file {File}: {Path}", fileName, path);
            return path;
        }

        public async Task SeedAsync()
        {
            await SeedWeaponTypesAsync();
            await SeedWeaponsAsync();
            await SeedWearTiersAsync();
            await SeedSkinsAsync();
            await SeedSkinWearTiersAsync();

            await SeedCaseHardenedGunPatternsAsync();
            await SeedCaseHardenedKnifePatternsAsync();

            await SeedFadeGunPatternsAsync();
            await SeedFadeKnifePatternsAsync();

            await SeedDopplerPhasesAsync();
            await SeedDopplerSkinPhasesAsync();

            await SeedStickersAndPricesFromDatasetAsync();
        }

        // ---------- weapon_types ----------

        private async Task SeedWeaponTypesAsync()
        {
            if (await _db.WeaponTypes.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] weapon_types already filled, skipping.");
                return;
            }

            var path = GetDataPath("weapon_types.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] weapon_types.csv not found at {Path}", path);
                return;
            }

            var lines = await File.ReadAllLinesAsync(path);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',', 2, StringSplitOptions.TrimEntries);
                if (parts.Length < 2) continue;

                var code = parts[0];
                var name = parts[1];

                _db.WeaponTypes.Add(new WeaponType
                {
                    Code = code,
                    Name = name
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] weapon_types seeded: {Count}", await _db.WeaponTypes.CountAsync());
        }

        // ---------- weapons ----------

        private async Task SeedWeaponsAsync()
        {
            if (await _db.Weapons.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] weapons already filled, skipping.");
                return;
            }

            var path = GetDataPath("weapons.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] weapons.csv not found at {Path}", path);
                return;
            }

            var weaponTypes = await _db.WeaponTypes.ToListAsync();
            var lines = await File.ReadAllLinesAsync(path);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',', 2, StringSplitOptions.TrimEntries);
                if (parts.Length < 2) continue;

                var name = parts[0];
                var typeCode = parts[1];

                var wt = weaponTypes.FirstOrDefault(t => t.Code == typeCode);
                if (wt == null)
                {
                    _logger.LogWarning("[DbSeeder] Unknown weapon_type_code {Code} for {Name}", typeCode, name);
                    continue;
                }

                _db.Weapons.Add(new Weapon
                {
                    Name = name,
                    WeaponTypeId = wt.Id
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] weapons seeded: {Count}", await _db.Weapons.CountAsync());
        }

        // ---------- wear_tiers ----------

        private async Task SeedWearTiersAsync()
        {
            if (await _db.WearTiers.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] wear_tiers already filled, skipping.");
                return;
            }

            var path = GetDataPath("wear_tiers.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] wear_tiers.csv not found at {Path}", path);
                return;
            }

            var lines = await File.ReadAllLinesAsync(path);
            foreach (var line in lines.Skip(1))
            {
                var name = line.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                _db.WearTiers.Add(new WearTier { Name = name });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] wear_tiers seeded: {Count}", await _db.WearTiers.CountAsync());
        }

        // ---------- skins ----------

        private async Task SeedSkinsAsync()
        {
            if (await _db.Skins.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] skins already filled, skipping.");
                return;
            }

            var path = GetDataPath("skins.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] skins.csv not found at {Path}", path);
                return;
            }

            var weapons = await _db.Weapons.ToListAsync();
            var lines = await File.ReadAllLinesAsync(path);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', 3, StringSplitOptions.TrimEntries);
                if (parts.Length < 3) continue;

                var weaponName = parts[0];
                var skinName = parts[1];
                var patternStyle = parts[2];

                var weapon = weapons.FirstOrDefault(w => w.Name == weaponName);
                if (weapon == null)
                {
                    _logger.LogWarning("[DbSeeder] Unknown weapon {Weapon} for skin {Skin}", weaponName, skinName);
                    continue;
                }

                _db.Skins.Add(new Skin
                {
                    WeaponId = weapon.Id,
                    Name = skinName,
                    PatternStyle = patternStyle
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] skins seeded: {Count}", await _db.Skins.CountAsync());
        }

        // ---------- skin_wear_tiers ----------

        private async Task SeedSkinWearTiersAsync()
        {
            if (await _db.SkinWearTiers.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] skin_wear_tiers already filled, skipping.");
                return;
            }

            var path = GetDataPath("skin_wear_tiers.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] skin_wear_tiers.csv not found at {Path}", path);
                return;
            }

            var skins = await _db.Skins.Include(s => s.Weapon).ToListAsync();
            var wearTiers = await _db.WearTiers.ToListAsync();

            var lines = await File.ReadAllLinesAsync(path);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', 3, StringSplitOptions.TrimEntries);
                if (parts.Length < 3) continue;

                var weaponName = parts[0];
                var skinName = parts[1];
                var wearName = parts[2];

                var skin = skins.FirstOrDefault(s =>
                    s.Weapon.Name == weaponName &&
                    s.Name == skinName);

                if (skin == null)
                {
                    _logger.LogWarning("[DbSeeder] Skin not found for {Weapon} | {Skin}", weaponName, skinName);
                    continue;
                }

                var wear = wearTiers.FirstOrDefault(w => w.Name == wearName);
                if (wear == null)
                {
                    _logger.LogWarning("[DbSeeder] Wear tier {Wear} not found", wearName);
                    continue;
                }

                if (await _db.SkinWearTiers.AnyAsync(x => x.SkinId == skin.Id && x.WearTierId == wear.Id))
                    continue;

                _db.SkinWearTiers.Add(new SkinWearTier
                {
                    SkinId = skin.Id,
                    WearTierId = wear.Id
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] skin_wear_tiers seeded: {Count}", await _db.SkinWearTiers.CountAsync());
        }

        // ---------- case_hardened_gun_patterns ----------

        private async Task SeedCaseHardenedGunPatternsAsync()
        {
            if (await _db.CaseHardenedGunPatterns.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] case_hardened_gun_patterns already filled, skipping.");
                return;
            }

            var path = GetDataPath("case_hardened_gun_unique_patterns.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] case_hardened_gun_patterns.csv not found at {Path}", path);
                return;
            }

            var skins = await _db.Skins.Include(s => s.Weapon).ToListAsync();
            var lines = await File.ReadAllLinesAsync(path);
            var culture = CultureInfo.InvariantCulture;
            var inserted = 0;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 5) continue;

                var weaponName = parts[0];
                var skinName = parts[1];

                if (!int.TryParse(parts[2], out var pattern))
                    continue;

                if (!double.TryParse(parts[3], NumberStyles.Any, culture, out var backsideBlue))
                    backsideBlue = 0;

                if (!double.TryParse(parts[4], NumberStyles.Any, culture, out var playsideBlue))
                    playsideBlue = 0;

                var skin = skins.FirstOrDefault(s =>
                    s.Weapon.Name == weaponName &&
                    s.Name == skinName);

                if (skin == null)
                {
                    _logger.LogWarning("[DbSeeder] CH gun skin not found {Weapon} | {Skin}", weaponName, skinName);
                    continue;
                }

                _db.CaseHardenedGunPatterns.Add(new CaseHardenedGunPattern
                {
                    SkinId = skin.Id,
                    Pattern = pattern,
                    PlaysideBlue = playsideBlue,
                    BacksideBlue = backsideBlue
                });

                inserted++;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] case_hardened_gun_patterns seeded: {Count}", inserted);
        }

        // ---------- case_hardened_knife_patterns ----------

        private async Task SeedCaseHardenedKnifePatternsAsync()
        {
            if (await _db.CaseHardenedKnifePatterns.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] case_hardened_knife_patterns already filled, skipping.");
                return;
            }

            var path = GetDataPath("case_hardened_knives_unique_patterns.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] case_hardened_knives_unique_patterns.csv not found at {Path}", path);
                return;
            }

            var skins = await _db.Skins.Include(s => s.Weapon).ToListAsync();
            var lines = await File.ReadAllLinesAsync(path);
            var culture = CultureInfo.InvariantCulture;
            var inserted = 0;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 9) continue;

                var weaponName = parts[0];
                var skinName = parts[1];

                if (!int.TryParse(parts[2], out var pattern))
                    continue;

                bool TryParseDouble(string s, out double value)
                    => double.TryParse(s, NumberStyles.Any, culture, out value);

                if (!TryParseDouble(parts[3], out var backsideBlue)) backsideBlue = 0;
                if (!TryParseDouble(parts[4], out var backsidePurple)) backsidePurple = 0;
                if (!TryParseDouble(parts[5], out var backsideGold)) backsideGold = 0;
                if (!TryParseDouble(parts[6], out var playsideBlue)) playsideBlue = 0;
                if (!TryParseDouble(parts[7], out var playsidePurple)) playsidePurple = 0;
                if (!TryParseDouble(parts[8], out var playsideGold)) playsideGold = 0;

                var skin = skins.FirstOrDefault(s =>
                    s.Weapon.Name == weaponName &&
                    s.Name == skinName);

                if (skin == null)
                {
                    _logger.LogWarning("[DbSeeder] CH knife skin not found {Weapon} | {Skin}", weaponName, skinName);
                    continue;
                }

                _db.CaseHardenedKnifePatterns.Add(new CaseHardenedKnifePattern
                {
                    SkinId = skin.Id,
                    Pattern = pattern,
                    BacksideBlue = backsideBlue,
                    BacksidePurple = backsidePurple,
                    BacksideGold = backsideGold,
                    PlaysideBlue = playsideBlue,
                    PlaysidePurple = playsidePurple,
                    PlaysideGold = playsideGold
                });

                inserted++;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] case_hardened_knife_patterns seeded: {Count}", inserted);
        }

        // ---------- fade_gun_patterns ----------

        private async Task SeedFadeGunPatternsAsync()
        {
            if (await _db.FadeGunPatterns.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] fade_gun_patterns already filled, skipping.");
                return;
            }

            var path = GetDataPath("fade_gun_unique_patterns.csv"); // 👈 ВАЖНО: имя как в папке data_for_db
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] fade_gun_unique_patterns.csv not found at {Path}", path);
                return;
            }

            var skins = await _db.Skins
                .Include(s => s.Weapon)
                .ToListAsync();

            var lines = await File.ReadAllLinesAsync(path);
            var culture = CultureInfo.InvariantCulture;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 5) continue;

                var weaponName = parts[0];      // AWP
                var skinName = parts[1];        // Fade

                if (!int.TryParse(parts[2], out var pattern))
                    continue;

                if (!double.TryParse(parts[3], NumberStyles.Any, culture, out var fadePercentage))
                    continue;

                if (!double.TryParse(parts[4], NumberStyles.Any, culture, out var fadeRank))
                    continue;

                var skin = skins.FirstOrDefault(s =>
                    s.Weapon.Name == weaponName &&
                    s.Name == skinName);

                if (skin == null)
                {
                    _logger.LogWarning("[DbSeeder] Fade gun skin not found {Weapon} | {Skin}", weaponName, skinName);
                    continue;
                }

                _db.FadeGunPatterns.Add(new FadeGunPattern
                {
                    SkinId = skin.Id,
                    Pattern = pattern,
                    FadePercentage = fadePercentage,
                    FadeRank = fadeRank
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] fade_gun_patterns seeded: {Count}", await _db.FadeGunPatterns.CountAsync());
        }


        // ---------- fade_knife_patterns ----------

        private async Task SeedFadeKnifePatternsAsync()
        {
            if (await _db.FadeKnifePatterns.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] fade_knife_patterns already filled, skipping.");
                return;
            }

            // ⚠️ ИСПОЛЬЗУЕМ fade_knives_unique_patterns.csv (knives!)
            var path = GetDataPath("fade_knives_unique_patterns.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] fade_knives_unique_patterns.csv not found at {Path}", path);
                return;
            }

            var skins = await _db.Skins.Include(s => s.Weapon).ToListAsync();
            var lines = await File.ReadAllLinesAsync(path);
            var culture = CultureInfo.InvariantCulture;
            var inserted = 0;

            // Ожидаемый формат:
            // weapon_name,skin_name,pattern,fade_percentage,fade_rank
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 5) continue;

                var weaponName = parts[0];
                var skinName = parts[1];

                if (!int.TryParse(parts[2], out var pattern))
                    continue;

                if (!double.TryParse(parts[3], NumberStyles.Any, culture, out var fadePercentage))
                    continue;

                if (!double.TryParse(parts[4], NumberStyles.Any, culture, out var fadeRank))
                    continue;

                var skin = skins.FirstOrDefault(s =>
                    s.Weapon.Name == weaponName &&
                    s.Name == skinName);

                if (skin == null)
                {
                    _logger.LogWarning("[DbSeeder] Fade knife skin not found {Weapon} | {Skin}", weaponName, skinName);
                    continue;
                }

                _db.FadeKnifePatterns.Add(new FadeKnifePattern
                {
                    SkinId = skin.Id,
                    Pattern = pattern,
                    FadePercentage = fadePercentage,
                    FadeRank = fadeRank
                });

                inserted++;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] fade_knife_patterns seeded: {Count}", inserted);
        }

        // ---------- doppler_phases ----------

        private async Task SeedDopplerPhasesAsync()
        {
            if (await _db.DopplerPhases.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] doppler_phases already filled, skipping.");
                return;
            }

            var path = GetDataPath("doppler_phases.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] doppler_phases.csv not found at {Path}", path);
                return;
            }

            var lines = await File.ReadAllLinesAsync(path);
            foreach (var line in lines.Skip(1))
            {
                var name = line.Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                _db.DopplerPhases.Add(new DopplerPhase { Name = name });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] doppler_phases seeded: {Count}", await _db.DopplerPhases.CountAsync());
        }

        // ---------- doppler_skin_phases ----------

        private async Task SeedDopplerSkinPhasesAsync()
        {
            if (await _db.DopplerSkinPhases.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] doppler_skin_phases already filled, skipping.");
                return;
            }

            var path = GetDataPath("doppler_skin_phases.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] doppler_skin_phases.csv not found at {Path}", path);
                return;
            }

            var skins = await _db.Skins.Include(s => s.Weapon).ToListAsync();
            var phases = await _db.DopplerPhases.ToListAsync();
            var lines = await File.ReadAllLinesAsync(path);
            var inserted = 0;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', 3, StringSplitOptions.TrimEntries);
                if (parts.Length < 3) continue;

                var weaponName = parts[0];
                var skinName = parts[1];
                var phaseName = parts[2];

                var skin = skins.FirstOrDefault(s =>
                    s.Weapon.Name == weaponName &&
                    s.Name == skinName);

                if (skin == null)
                {
                    _logger.LogWarning("[DbSeeder] Doppler skin not found {Weapon} | {Skin}", weaponName, skinName);
                    continue;
                }

                var phase = phases.FirstOrDefault(p => p.Name == phaseName);
                if (phase == null)
                {
                    _logger.LogWarning("[DbSeeder] Doppler phase not found: {Phase}", phaseName);
                    continue;
                }

                if (await _db.DopplerSkinPhases.AnyAsync(x => x.SkinId == skin.Id && x.PhaseId == phase.Id))
                    continue;

                _db.DopplerSkinPhases.Add(new DopplerSkinPhase
                {
                    SkinId = skin.Id,
                    PhaseId = phase.Id
                });

                inserted++;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[DbSeeder] doppler_skin_phases seeded: {Count}", inserted);
        }

        // ---------- stickers + sticker_prices из stickers_dataset.csv ----------

        private async Task SeedStickersAndPricesFromDatasetAsync()
        {
            if (await _db.Stickers.AnyAsync() || await _db.StickerPrices.AnyAsync())
            {
                _logger.LogInformation("[DbSeeder] stickers / sticker_prices already filled, skipping.");
                return;
            }

            var path = GetDataPath("stickers_dataset.csv");
            if (!File.Exists(path))
            {
                _logger.LogWarning("[DbSeeder] stickers_dataset.csv not found at {Path}", path);
                return;
            }

            var lines = await File.ReadAllLinesAsync(path);
            var culture = CultureInfo.InvariantCulture;

            // Формат:
            // sticker_id,name,reference_price
            // 114,Sticker | Sneaky Beaky Like,0.36

            var stickersByName = _db.Stickers.Local.ToDictionary(s => s.Name, s => s);
            int stickersInserted = 0;
            int pricesInserted = 0;

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',', 3, StringSplitOptions.TrimEntries);
                if (parts.Length < 3) continue;

                // var externalId = parts[0]; // можем игнорить
                var name = parts[1];

                if (!double.TryParse(parts[2], NumberStyles.Any, culture, out var price))
                    continue;

                if (!stickersByName.TryGetValue(name, out var sticker))
                {
                    sticker = new Sticker { Name = name };
                    _db.Stickers.Add(sticker);
                    stickersByName[name] = sticker;
                    stickersInserted++;
                }

                _db.StickerPrices.Add(new StickerPrice
                {
                    Sticker = sticker,
                    Price = price
                });

                pricesInserted++;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "[DbSeeder] stickers_dataset seeded: stickers={Stickers}, prices={Prices}",
                stickersInserted, pricesInserted
            );
        }
    }
}
