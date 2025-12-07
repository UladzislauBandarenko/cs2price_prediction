using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.DTOs.AI;
using cs2price_prediction.DTOs.AI.CaseHardenedKnife;
using cs2price_prediction.DTOs.AI.ChGuns;
using cs2price_prediction.DTOs.AI.Doppler;
using cs2price_prediction.DTOs.AI.FadeGun;
using cs2price_prediction.DTOs.AI.FadeKnife;
using cs2price_prediction.DTOs.AI.FloatGuns;
using cs2price_prediction.DTOs.AI.StickersDtoForAI;
using cs2price_prediction.Services.AI.AiPromptService;
using cs2price_prediction.Services.AI.AiStickerService;
using cs2price_prediction.Services.AI.Llm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Services.AI.AiExplanation
{
    public class AiExplanationService : IAiExplanationService
    {
        private readonly AppDbContext _db;
        private readonly IAiStickerService _aiStickerService;
        private readonly IAiPromptFactory _promptFactory;
        private readonly ILLMClient _llmClient;

        public AiExplanationService(
            AppDbContext db,
            IAiStickerService aiStickerService,
            IAiPromptFactory promptFactory,
            ILLMClient llmClient)
        {
            _db = db;
            _aiStickerService = aiStickerService;
            _promptFactory = promptFactory;
            _llmClient = llmClient;
        }

        /// <summary>
        /// Главный вход: строим данные для AI, промпт и вызываем LLM с учётом приоритета модели.
        /// </summary>
        public async Task<IActionResult> ExplainAsync(AiExplainFrontendInputDto dto, LlmPriority priority)
        {
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

            // ----------------------------
            // STICKER LOGIC
            // ----------------------------

            var isKnife =
                patternStyle == "ch_knife" ||
                patternStyle == "fade_knife" ||
                patternStyle == "doppler_knife";

            StickersDtoForAI stickersForAi;

            if (isKnife)
            {
                // НОЖИ: полностью игнорируем стикеры
                stickersForAi = new StickersDtoForAI
                {
                    Slot0Price = 0,
                    Slot1Price = 0,
                    Slot2Price = 0,
                    Slot3Price = 0,
                    StickerSlot1Name = null,
                    StickerSlot2Name = null,
                    StickerSlot3Name = null,
                    StickerSlot4Name = null
                };
            }
            else
            {
                var stickerIds = dto.Stickers ?? new List<int>();
                stickersForAi = await _aiStickerService.BuildStickersDtoForAiAsync(stickerIds);
            }

            // ----------------------------
            // ROUTING BY pattern_style
            // ----------------------------

            return patternStyle switch
            {
                "ch_knife" =>
                    await ExplainCaseHardenedKnife(dto, weaponName, skinName, wearName, priority),

                "ch_gun" =>
                    await ExplainCaseHardenedGun(dto, weaponName, skinName, wearName, stickersForAi, priority),

                "fade_gun" =>
                    await ExplainFadeGun(dto, weaponName, skinName, wearName, stickersForAi, priority),

                "fade_knife" =>
                    await ExplainFadeKnife(dto, weaponName, skinName, wearName, priority),

                "doppler_knife" =>
                    await ExplainDopplerKnife(dto, skin.Id, weaponName, skinName, wearName, priority),

                "float_gun" =>
                    await ExplainFloatSensitiveGuns(dto, weaponName, skinName, wearName, stickersForAi, priority),

                _ => new BadRequestObjectResult($"Unsupported pattern style for AI: {patternStyle}")
            };
        }

        // -------------------------
        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ВЫЗОВА LLM С ПРИОРИТЕТОМ
        // -------------------------

        private async Task<string> QueryWithPriorityAsync(string prompt, LlmPriority priority)
        {
            // Имена моделей: подгони под свои реальные, если отличаются
            const string MiniModel = "gpt-4o-mini";
            const string BigModel = "gpt-4.1-mini";

            switch (priority)
            {
                case LlmPriority.MiniThenGpt41:
                    {
                        // Сначала mini, если упала — big
                        try
                        {
                            return await _llmClient.QueryAsync(prompt, MiniModel);
                        }
                        catch
                        {
                            return await _llmClient.QueryAsync(prompt, BigModel);
                        }
                    }

                case LlmPriority.Gpt41ThenMini:
                    {
                        // Сначала big, если упала — mini
                        try
                        {
                            return await _llmClient.QueryAsync(prompt, BigModel);
                        }
                        catch
                        {
                            return await _llmClient.QueryAsync(prompt, MiniModel);
                        }
                    }

                default:
                    // На всякий случай — по умолчанию mini
                    return await _llmClient.QueryAsync(prompt, MiniModel);
            }
        }

        // -------------------------
        // ch_knife
        // -------------------------
        private async Task<IActionResult> ExplainCaseHardenedKnife(
            AiExplainFrontendInputDto dto,
            string weapon,
            string skin,
            string wear,
            LlmPriority priority)
        {
            var row = await _db.CaseHardenedKnifePatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found for this skin (ch_knife).");

            var request = new AiCaseHardenedKnifeRequest
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
                Wear = wear,

                PredictedPrice = dto.PredictedPrice
            };

            var prompt = _promptFactory.BuildCaseHardenedKnifePrompt(request);
            var explanation = await QueryWithPriorityAsync(prompt, priority);
            return new OkObjectResult(new { explanation });
        }

        // -------------------------
        // ch_gun
        // -------------------------
        private async Task<IActionResult> ExplainCaseHardenedGun(
            AiExplainFrontendInputDto dto,
            string weapon,
            string skin,
            string wear,
            StickersDtoForAI stickers,
            LlmPriority priority)
        {
            var row = await _db.CaseHardenedGunPatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found for this skin (ch_gun).");

            var blueScore = row.PlaysideBlue + row.BacksideBlue;

            var request = new AiChGunsRequest
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
                BlueScore = blueScore,
                BlueTier =
                    blueScore >= 100 ? 4 :
                    blueScore >= 70 ? 3 :
                    blueScore >= 40 ? 2 : 1,

                Slot0Price = stickers.Slot0Price,
                Slot1Price = stickers.Slot1Price,
                Slot2Price = stickers.Slot2Price,
                Slot3Price = stickers.Slot3Price,

                StickerSlot1Name = stickers.StickerSlot1Name,
                StickerSlot2Name = stickers.StickerSlot2Name,
                StickerSlot3Name = stickers.StickerSlot3Name,
                StickerSlot4Name = stickers.StickerSlot4Name,

                PredictedPrice = dto.PredictedPrice
            };

            var prompt = _promptFactory.BuildChGunsPrompt(request);
            var explanation = await QueryWithPriorityAsync(prompt, priority);

            return new OkObjectResult(new { explanation });
        }

        // -------------------------
        // doppler_knife
        // -------------------------
        private async Task<IActionResult> ExplainDopplerKnife(
            AiExplainFrontendInputDto dto,
            int skinId,
            string weapon,
            string skin,
            string wear,
            LlmPriority priority)
        {
            var link = await _db.DopplerSkinPhases
                .Include(d => d.Phase)
                .FirstOrDefaultAsync(d =>
                    d.SkinId == skinId &&
                    d.PhaseId == dto.Pattern);

            if (link is null)
                return new BadRequestObjectResult("Doppler phase not found.");

            var request = new AiDopplerRequest
            {
                Weapon = weapon,
                Skin = skin,
                Wear = wear,

                Phase = link.Phase.Name,
                Float = dto.FloatValue,
                Stattrak = dto.IsStattrak ? 1 : 0,

                PredictedPrice = dto.PredictedPrice
            };

            var prompt = _promptFactory.BuildDopplerPrompt(request);
            var explanation = await QueryWithPriorityAsync(prompt, priority);

            return new OkObjectResult(new { explanation });
        }

        // -------------------------
        // fade_gun
        // -------------------------
        private async Task<IActionResult> ExplainFadeGun(
            AiExplainFrontendInputDto dto,
            string weapon,
            string skin,
            string wear,
            StickersDtoForAI stickers,
            LlmPriority priority)
        {
            var row = await _db.FadeGunPatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found (fade_gun).");

            var request = new AiFadeGunsRequest
            {
                Float = dto.FloatValue,
                Pattern = dto.Pattern,
                Stattrak = dto.IsStattrak ? 1 : 0,

                FadePercentage = row.FadePercentage,
                FadeRank = row.FadeRank,

                Slot0Price = stickers.Slot0Price,
                Slot1Price = stickers.Slot1Price,
                Slot2Price = stickers.Slot2Price,
                Slot3Price = stickers.Slot3Price,

                StickerSlot1Name = stickers.StickerSlot1Name,
                StickerSlot2Name = stickers.StickerSlot2Name,
                StickerSlot3Name = stickers.StickerSlot3Name,
                StickerSlot4Name = stickers.StickerSlot4Name,

                Weapon = weapon,
                Skin = skin,
                Wear = wear,

                PredictedPrice = dto.PredictedPrice
            };

            var prompt = _promptFactory.BuildFadeGunsPrompt(request);
            var explanation = await QueryWithPriorityAsync(prompt, priority);

            return new OkObjectResult(new { explanation });
        }

        // -------------------------
        // fade_knife
        // -------------------------
        private async Task<IActionResult> ExplainFadeKnife(
            AiExplainFrontendInputDto dto,
            string weapon,
            string skin,
            string wear,
            LlmPriority priority)
        {
            var row = await _db.FadeKnifePatterns
                .FirstOrDefaultAsync(p => p.SkinId == dto.SkinId && p.Pattern == dto.Pattern);

            if (row is null)
                return new BadRequestObjectResult("Pattern not found (fade_knife).");

            var request = new AiFadeKnivesRequest
            {
                Float = dto.FloatValue,
                Pattern = dto.Pattern,
                Stattrak = dto.IsStattrak ? 1 : 0,

                FadePercentage = row.FadePercentage,
                FadeRank = row.FadeRank,

                Weapon = weapon,
                Skin = skin,
                Wear = wear,

                PredictedPrice = dto.PredictedPrice
            };

            var prompt = _promptFactory.BuildFadeKnivesPrompt(request);
            var explanation = await QueryWithPriorityAsync(prompt, priority);

            return new OkObjectResult(new { explanation });
        }

        // -------------------------
        // float_gun
        // -------------------------
        private async Task<IActionResult> ExplainFloatSensitiveGuns(
            AiExplainFrontendInputDto dto,
            string weapon,
            string skin,
            string wear,
            StickersDtoForAI stickers,
            LlmPriority priority)
        {
            var request = new AiFloatSensitiveGunsRequest
            {
                Float = dto.FloatValue,
                Stattrak = dto.IsStattrak ? 1 : 0,

                Weapon = weapon,
                Skin = skin,
                Wear = wear,

                Slot0Price = stickers.Slot0Price,
                Slot1Price = stickers.Slot1Price,
                Slot2Price = stickers.Slot2Price,
                Slot3Price = stickers.Slot3Price,

                StickerSlot1Name = stickers.StickerSlot1Name,
                StickerSlot2Name = stickers.StickerSlot2Name,
                StickerSlot3Name = stickers.StickerSlot3Name,
                StickerSlot4Name = stickers.StickerSlot4Name,

                PredictedPrice = dto.PredictedPrice
            };

            var prompt = _promptFactory.BuildFloatGunsPrompt(request);
            var explanation = await QueryWithPriorityAsync(prompt, priority);

            return new OkObjectResult(new { explanation });
        }
    }
}
