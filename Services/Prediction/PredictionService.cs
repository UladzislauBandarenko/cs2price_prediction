using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.Ml;
using cs2price_prediction.DTOs.Prediction;
using cs2price_prediction.Services.Stickers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.Prediction
{
    public class PredictionService : IPredictionService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStickerService _stickerService;

        public PredictionService(
            AppDbContext db,
            IHttpClientFactory httpClientFactory,
            IStickerService stickerService)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _stickerService = stickerService;
        }

        public async Task<IActionResult> PredictAsync(PredictionRequestDto dto)
        {
            // 1. Skin + Weapon + PatternStyle
            var skin = await _db.Skins
                .Include(s => s.Weapon)
                .FirstOrDefaultAsync(s => s.Id == dto.SkinId);

            if (skin is null)
                return new NotFoundObjectResult("Skin not found.");

            var wear = await _db.WearTiers
                .FirstOrDefaultAsync(w => w.Id == dto.WearTierId);

            if (wear is null)
                return new BadRequestObjectResult("Wear tier not found.");

            var wearAllowed = await _db.SkinWearTiers
                .AnyAsync(sw => sw.SkinId == dto.SkinId && sw.WearTierId == dto.WearTierId);

            if (!wearAllowed)
                return new BadRequestObjectResult("This wear tier is not available for the selected skin.");

            var patternStyle = skin.PatternStyle;
            var weaponName = skin.Weapon.Name;
            var skinName = skin.Name;
            var wearName = wear.Name;

            // 2. HttpClient (один на весь запрос)
            var client = _httpClientFactory.CreateClient("MlService");

            // 3. Стикеры (считаем один раз)
            var stickerIds = dto.Stickers ?? new List<int>();
            var stickerFeatures = await _stickerService.CalculateFeaturesAsync(stickerIds);

            // 4. Роутинг по pattern_style → к конкретному ML endpoint
            return patternStyle switch
            {
                "ch_knife" => await PredictCaseHardenedKnife(dto, weaponName, skinName, wearName, client),
                "ch_gun" => await PredictCaseHardenedGun(dto, weaponName, skinName, wearName, stickerFeatures, client),
                "fade_gun" => await PredictFadeGun(dto, weaponName, skinName, wearName, stickerFeatures, client),
                "fade_knife" => await PredictFadeKnife(dto, weaponName, skinName, wearName, client),
                "doppler_knife" => await PredictDopplerKnife(dto, skin.Id, weaponName, skinName, wearName, client),
                "float_gun" => await PredictFloatSensitiveGuns(dto, weaponName, skinName, wearName, stickerFeatures, client),

                _ => new BadRequestObjectResult($"Unsupported pattern style: {patternStyle}")
            };
        }

        // ---------------------------
        // /predict/case-hardened (ch_knife)
        // ---------------------------
        private async Task<IActionResult> PredictCaseHardenedKnife(
            PredictionRequestDto dto,
            string weapon,
            string skin,
            string wear,
            HttpClient client)
        {
            var row = await _db.CaseHardenedKnifePatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found for this skin (ch_knife).");

            var mlRequest = new MlCaseHardenedKnifeRequest
            {
                Float = dto.FloatValue,
                Pattern = dto.Pattern,
                Stattrak = dto.IsStattrak ? 1 : 0,

                BacksideBlue = row.BacksideBlue,
                BacksidePurple = row.BacksidePurple ?? 0,
                BacksideGold = row.BacksideGold ?? 0,

                PlaysideBlue = row.PlaysideBlue,
                PlaysidePurple = row.PlaysidePurple ?? 0,
                PlaysideGold = row.PlaysideGold ?? 0,

                Weapon = weapon,
                Skin = skin,
                Wear = wear
            };

            var response = await client.PostAsJsonAsync("/predict/case-hardened", mlRequest);
            return await MapMlResponse(response);
        }

        // ---------------------------
        // /predict/ch-guns (ch_gun)
        // ---------------------------
        private async Task<IActionResult> PredictCaseHardenedGun(
            PredictionRequestDto dto,
            string weapon,
            string skin,
            string wear,
            StickerFeatures sf,
            HttpClient client)
        {
            var row = await _db.CaseHardenedGunPatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found for this skin (ch_gun).");

            var blueScore = row.PlaysideBlue + row.BacksideBlue;
            double blueTier =
                blueScore >= 100 ? 4 :
                blueScore >= 70 ? 3 :
                blueScore >= 40 ? 2 : 1;

            var mlRequest = new MlChGunsRequest
            {
                Weapon = weapon,
                Skin = skin,
                Wear = wear,
                PatternStyle = "ch_gun",

                Float = dto.FloatValue,
                Pattern = dto.Pattern,
                Stattrak = dto.IsStattrak ? 1 : 0,

                BacksideBlue = row.BacksideBlue,
                PlaysideBlue = row.PlaysideBlue,

                StickersCount = sf.StickersCount,
                StickersTotalValue = sf.StickersTotalValue,
                StickersAvgValue = sf.StickersAvgValue,
                StickersMaxValue = sf.StickersMaxValue,
                Slot0Price = sf.Slot0Price,
                Slot1Price = sf.Slot1Price,
                Slot2Price = sf.Slot2Price,
                Slot3Price = sf.Slot3Price,

                BlueScore = blueScore,
                BlueTier = blueTier
            };

            var response = await client.PostAsJsonAsync("/predict/ch-guns", mlRequest);
            return await MapMlResponse(response);
        }

        // ---------------------------
        // /predict/doppler (doppler_knife)
        // ---------------------------
        private async Task<IActionResult> PredictDopplerKnife(
            PredictionRequestDto dto,
            int skinId,
            string weapon,
            string skin,
            string wear,
            HttpClient client)
        {
            // dto.Pattern = PhaseId
            var dopplerLink = await _db.DopplerSkinPhases
                .Include(d => d.Phase)
                .FirstOrDefaultAsync(d => d.SkinId == skinId && d.PhaseId == dto.Pattern);

            if (dopplerLink is null)
                return new BadRequestObjectResult("Doppler phase not found for this skin (Pattern as PhaseId).");

            var phaseName = dopplerLink.Phase.Name;

            var mlRequest = new MlDopplerRequest
            {
                Weapon = weapon,
                Skin = skin,
                Wear = wear,
                Phase = phaseName,
                Float = dto.FloatValue,
                Stattrak = dto.IsStattrak ? 1 : 0
            };

            var response = await client.PostAsJsonAsync("/predict/doppler", mlRequest);
            return await MapMlResponse(response);
        }

        // ---------------------------
        // /predict/fade-guns (fade_gun)
        // ---------------------------
        private async Task<IActionResult> PredictFadeGun(
            PredictionRequestDto dto,
            string weapon,
            string skin,
            string wear,
            StickerFeatures sf,
            HttpClient client)
        {
            var row = await _db.FadeGunPatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found for this skin (fade_gun).");

            var mlRequest = new MlFadeGunsRequest
            {
                Float = dto.FloatValue,
                Pattern = dto.Pattern,
                Stattrak = dto.IsStattrak ? 1 : 0,

                FadePercentage = row.FadePercentage,
                FadeRank = row.FadeRank,

                StickersCount = sf.StickersCount,
                StickersTotalValue = sf.StickersTotalValue,
                StickersAvgValue = sf.StickersAvgValue,
                StickersMaxValue = sf.StickersMaxValue,
                Slot0Price = sf.Slot0Price,
                Slot1Price = sf.Slot1Price,
                Slot2Price = sf.Slot2Price,
                Slot3Price = sf.Slot3Price,

                Weapon = weapon,
                Skin = skin,
                Wear = wear
            };

            var response = await client.PostAsJsonAsync("/predict/fade-guns", mlRequest);
            return await MapMlResponse(response);
        }

        // ---------------------------
        // /predict/fade-knives (fade_knife)
        // ---------------------------
        private async Task<IActionResult> PredictFadeKnife(
            PredictionRequestDto dto,
            string weapon,
            string skin,
            string wear,
            HttpClient client)
        {
            var row = await _db.FadeKnifePatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found for this skin (fade_knife).");

            var mlRequest = new MlFadeKnivesRequest
            {
                Float = dto.FloatValue,
                Pattern = dto.Pattern,
                Stattrak = dto.IsStattrak ? 1 : 0,

                FadePercentage = row.FadePercentage,
                FadeRank = row.FadeRank,

                Weapon = weapon,
                Skin = skin,
                Wear = wear
            };

            var response = await client.PostAsJsonAsync("/predict/fade-knives", mlRequest);
            return await MapMlResponse(response);
        }

        // ---------------------------
        // /predict/float-sensitive-guns (float_gun)
        // ---------------------------
        private async Task<IActionResult> PredictFloatSensitiveGuns(
            PredictionRequestDto dto,
            string weapon,
            string skin,
            string wear,
            StickerFeatures sf,
            HttpClient client)
        {
            var mlRequest = new MlFloatSensitiveGunsRequest
            {
                Float = dto.FloatValue,
                Stattrak = dto.IsStattrak ? 1 : 0,

                StickersCount = sf.StickersCount,
                StickersTotalValue = sf.StickersTotalValue,
                StickersAvgValue = sf.StickersAvgValue,
                StickersMaxValue = sf.StickersMaxValue,
                Slot0Price = sf.Slot0Price,
                Slot1Price = sf.Slot1Price,
                Slot2Price = sf.Slot2Price,
                Slot3Price = sf.Slot3Price,

                Weapon = weapon,
                Skin = skin,
                Wear = wear
            };

            var response = await client.PostAsJsonAsync("/predict/float-sensitive-guns", mlRequest);
            return await MapMlResponse(response);
        }

        // ---------------------------
        // Общий маппинг ответа от ML
        // ---------------------------
        private async Task<IActionResult> MapMlResponse(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new ObjectResult(body)
                {
                    StatusCode = (int)response.StatusCode
                };

            var result = await response.Content.ReadFromJsonAsync<MlPredictionResponse>();
            if (result is null)
                return new ObjectResult("Invalid response from ML service.")
                {
                    StatusCode = 500
                };

            return new OkObjectResult(result);
        }
    }
}
