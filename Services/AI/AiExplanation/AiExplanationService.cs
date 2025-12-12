// File: Services/AI/AiExplanation/AiExplanationService.cs
using System.Collections.Generic;
using System.Linq;
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
        /// Main entry: prepare data for AI, build prompt and query LLM according to priority.
        /// - Validates PredictedPrice: it must be greater than 0 (non-zero).
        /// - Validates wear / skin existence and allowed wear tiers.
        /// - Validates floatValue matches wear-tier ranges.
        /// - Builds sticker DTOs (knives ignore stickers).
        /// - Enforces strict validation: maximum allowed stickers is 4 for non-knife items.
        /// </summary>
        public async Task<IActionResult> ExplainAsync(AiExplainFrontendInputDto dto, LlmPriority priority)
        {
            // Validate predicted price: cannot be zero or negative.
            if (dto.PredictedPrice <= 0)
            {
                return new BadRequestObjectResult("PredictedPrice must be greater than 0. Provide a valid non-zero predicted price.");
            }

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
            // FLOAT vs WEAR validation
            // ----------------------------
            // Define allowed ranges per wear tier and validate incoming float value.
            // Ranges (inclusive):
            // Factory New:    0.00  - 0.07
            // Minimal Wear:   0.07  - 0.15
            // Field-Tested:   0.15  - 0.38
            // Well-Worn:      0.38  - 0.45
            // Battle-Scarred: 0.45  - 1.00

            (double Min, double Max, bool Found) GetWearRange(string wName) =>
                wName switch
                {
                    "Factory New" => (0.00, 0.07, true),
                    "Minimal Wear" => (0.07, 0.15, true),
                    "Field-Tested" => (0.15, 0.38, true),
                    "Well-Worn" => (0.38, 0.45, true),
                    "Battle-Scarred" => (0.45, 1.00, true),
                    _ => (0.0, 0.0, false)
                };

            var (minAllowed, maxAllowed, foundRange) = GetWearRange(wearName);

            if (!foundRange)
            {
                // If wear name is unexpected — treat as bad request.
                return new BadRequestObjectResult($"Unknown wear tier name: '{wearName}'.");
            }

            // Float value should be within the allowed inclusive range.
            if (dto.FloatValue < minAllowed || dto.FloatValue > maxAllowed)
            {
                return new BadRequestObjectResult(
                    $"Float value {dto.FloatValue} is invalid for wear tier '{wearName}'. Allowed range: [{minAllowed:F2} - {maxAllowed:F2}]."
                );
            }

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
                // For knives we ignore sticker prices/names (they don't affect knife explanation).
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

                // Strict validation: reject requests with more than 4 stickers for non-knife items.
                if (stickerIds.Count > 4)
                {
                    return new BadRequestObjectResult("Maximum 4 stickers are allowed. Provide 4 or fewer sticker IDs.");
                }

                // Build DTO for AI (preserves order and duplicates as implemented in IAiStickerService).
                stickersForAi = await BuildStickersDtoForAiWrapperAsync(stickerIds);
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

        /// <summary>
        /// Wrapper that converts IReadOnlyCollection<int> to IReadOnlyList<int> (List<int>)
        /// because IAiStickerService.BuildStickersDtoForAiAsync expects IReadOnlyList<int>.
        /// This preserves order and duplicates.
        /// </summary>
        private async Task<StickersDtoForAI> BuildStickersDtoForAiWrapperAsync(IReadOnlyCollection<int> stickerIds)
        {
            if (stickerIds is IReadOnlyList<int> readOnlyList)
            {
                return await _aiStickerService.BuildStickersDtoForAiAsync(readOnlyList);
            }

            var list = stickerIds.ToList();
            return await _aiStickerService.BuildStickersDtoForAiAsync(list);
        }

        // -------------------------
        // Helper to query LLM with priority fallback
        // -------------------------
        private async Task<string> QueryWithPriorityAsync(string prompt, LlmPriority priority)
        {
            // Model names - adjust to your actual available models if different
            const string MiniModel = "gpt-4o-mini";
            const string BigModel = "gpt-4.1-mini";

            switch (priority)
            {
                case LlmPriority.MiniThenGpt41:
                    try
                    {
                        return await _llmClient.QueryAsync(prompt, MiniModel);
                    }
                    catch
                    {
                        return await _llmClient.QueryAsync(prompt, BigModel);
                    }

                case LlmPriority.Gpt41ThenMini:
                    try
                    {
                        return await _llmClient.QueryAsync(prompt, BigModel);
                    }
                    catch
                    {
                        return await _llmClient.QueryAsync(prompt, MiniModel);
                    }

                default:
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
                Slot1Price = stickers.StickerSlot1Name != null ? stickers.Slot1Price : 0,
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
