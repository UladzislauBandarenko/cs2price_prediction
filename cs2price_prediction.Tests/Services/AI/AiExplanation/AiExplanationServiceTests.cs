using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.AI;
using cs2price_prediction.DTOs.AI.CaseHardenedKnife;
using cs2price_prediction.DTOs.AI.ChGuns;
using cs2price_prediction.DTOs.AI.Doppler;
using cs2price_prediction.DTOs.AI.FadeGun;
using cs2price_prediction.DTOs.AI.FadeKnife;
using cs2price_prediction.DTOs.AI.FloatGuns;
using cs2price_prediction.DTOs.AI.StickersDtoForAI;
using cs2price_prediction.Services.AI.AiExplanation;
using cs2price_prediction.Services.AI.AiPromptService;
using cs2price_prediction.Services.AI.AiStickerService;
using cs2price_prediction.Services.AI.Llm;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.AI.AiExplanation
{
    public class AiExplanationServiceTests
    {
        private static DbContextOptions<AppDbContext> CreateOptions()
            => new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private static string GetExplanation(IActionResult result)
        {
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull("result should be OkObjectResult");
            ok!.Value.Should().NotBeNull();

            var value = ok.Value!;
            var prop = value.GetType().GetProperty("explanation");
            prop.Should().NotBeNull("anonymous object should contain 'explanation' property");

            var expl = prop!.GetValue(value) as string;
            expl.Should().NotBeNull();
            return expl!;
        }

        private AiExplanationService CreateService(
            AppDbContext db,
            out Mock<IAiStickerService> stickerMock,
            out Mock<IAiPromptFactory> promptFactoryMock,
            out Mock<ILLMClient> llmMock)
        {
            stickerMock = new Mock<IAiStickerService>();
            promptFactoryMock = new Mock<IAiPromptFactory>();
            llmMock = new Mock<ILLMClient>();

            return new AiExplanationService(db, stickerMock.Object, promptFactoryMock.Object, llmMock.Object);
        }

        // ---------------------------------------------------------------------
        // Basic validation
        // ---------------------------------------------------------------------

        [Fact]
        public async Task ExplainAsync_Returns_NotFound_When_Skin_Not_Found()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var service = CreateService(db, out var stickerMock, out var promptFactoryMock, out var llmMock);

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 1,
                Pattern = 1,
                FloatValue = 0.1,
                IsStattrak = false,
                PredictedPrice = 100,
                Stickers = new List<int> { 1, 2 }
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            result.Should().BeOfType<NotFoundObjectResult>();
            (result as NotFoundObjectResult)!.Value.Should().Be("Skin not found.");

            stickerMock.VerifyNoOtherCalls();
            promptFactoryMock.VerifyNoOtherCalls();
            llmMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExplainAsync_Returns_BadRequest_When_WearTier_Not_Found()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var weapon = new Weapon { Id = 10, Name = "AK-47", WeaponTypeId = 1 };
            db.Weapons.Add(weapon);
            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "Redline",
                PatternStyle = "ch_gun",
                WeaponId = weapon.Id,
                Weapon = weapon
            });
            await db.SaveChangesAsync();

            var service = CreateService(db, out _, out _, out _);

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 5,
                Pattern = 1,
                FloatValue = 0.1,
                IsStattrak = false,
                PredictedPrice = 150
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be("Wear tier not found.");
        }

        [Fact]
        public async Task ExplainAsync_Returns_BadRequest_When_Wear_Not_Allowed_For_Skin()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var weapon = new Weapon { Id = 10, Name = "AK-47", WeaponTypeId = 1 };
            db.Weapons.Add(weapon);

            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "Redline",
                PatternStyle = "ch_gun",
                WeaponId = weapon.Id,
                Weapon = weapon
            });

            db.WearTiers.Add(new WearTier
            {
                Id = 3,
                Name = "Field-Tested"
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, out _, out _, out _);

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 3,
                Pattern = 1,
                FloatValue = 0.1,
                IsStattrak = false,
                PredictedPrice = 150
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should().Be("This wear tier is not available for the selected skin.");
        }

        [Fact]
        public async Task ExplainAsync_Returns_BadRequest_For_Unsupported_PatternStyle()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var weapon = new Weapon { Id = 10, Name = "Glock-18", WeaponTypeId = 1 };
            db.Weapons.Add(weapon);

            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "Test Skin",
                PatternStyle = "unknown_style",
                WeaponId = weapon.Id,
                Weapon = weapon
            });

            db.WearTiers.Add(new WearTier { Id = 1, Name = "Factory New" });
            db.SkinWearTiers.Add(new SkinWearTier
            {
                SkinId = 1,
                WearTierId = 1
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, out _, out _, out _);

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 1,
                Pattern = 777,
                FloatValue = 0.2,
                IsStattrak = true,
                PredictedPrice = 300
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            result.Should().BeOfType<BadRequestObjectResult>();
            (result as BadRequestObjectResult)!.Value.Should()
                .Be("Unsupported pattern style for AI: unknown_style");
        }

        // ---------------------------------------------------------------------
        // ch_knife: игнор стикеров + базовый Ok
        // ---------------------------------------------------------------------

        [Fact]
        public async Task ExplainAsync_ChKnife_Ignores_Stickers_And_Returns_Ok()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var weapon = new Weapon { Id = 10, Name = "Bayonet", WeaponTypeId = 1 };
            db.Weapons.Add(weapon);

            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "Case Hardened",
                PatternStyle = "ch_knife",
                WeaponId = weapon.Id,
                Weapon = weapon
            });

            db.WearTiers.Add(new WearTier { Id = 1, Name = "Factory New" });
            db.SkinWearTiers.Add(new SkinWearTier
            {
                SkinId = 1,
                WearTierId = 1
            });

            db.CaseHardenedKnifePatterns.Add(new CaseHardenedKnifePattern
            {
                SkinId = 1,
                Pattern = 777,
                BacksideBlue = 10,
                BacksidePurple = 20,
                BacksideGold = 30,
                PlaysideBlue = 40,
                PlaysidePurple = 50,
                PlaysideGold = 60
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, out var stickerMock, out var promptFactoryMock, out var llmMock);

            promptFactoryMock
                .Setup(p => p.BuildCaseHardenedKnifePrompt(It.IsAny<AiCaseHardenedKnifeRequest>()))
                .Returns("PROMPT_CH_KNIFE");

            llmMock
                .Setup(l => l.QueryAsync("PROMPT_CH_KNIFE", It.IsAny<string>()))
                .ReturnsAsync("EXPLANATION_CH_KNIFE");

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 1,
                Pattern = 777,
                FloatValue = 0.03,
                IsStattrak = true,
                PredictedPrice = 1000,
                Stickers = new List<int> { 1, 2, 3, 4 }
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            var explanation = GetExplanation(result);
            explanation.Should().Be("EXPLANATION_CH_KNIFE");

            // стикеры для ножей игнорируются
            stickerMock.Verify(
                s => s.BuildStickersDtoForAiAsync(It.IsAny<IReadOnlyList<int>>()),
                Times.Never);
        }

        // ---------------------------------------------------------------------
        // ch_gun: стикеры используются + Ok
        // ---------------------------------------------------------------------

        [Fact]
        public async Task ExplainAsync_ChGun_Uses_Stickers_And_Returns_Ok()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var weapon = new Weapon { Id = 10, Name = "AK-47", WeaponTypeId = 1 };
            db.Weapons.Add(weapon);

            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "Case Hardened",
                PatternStyle = "ch_gun",
                WeaponId = weapon.Id,
                Weapon = weapon
            });

            db.WearTiers.Add(new WearTier { Id = 1, Name = "Factory New" });
            db.SkinWearTiers.Add(new SkinWearTier
            {
                SkinId = 1,
                WearTierId = 1
            });

            db.CaseHardenedGunPatterns.Add(new CaseHardenedGunPattern
            {
                SkinId = 1,
                Pattern = 123,
                BacksideBlue = 30,
                PlaysideBlue = 40
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, out var stickerMock, out var promptFactoryMock, out var llmMock);

            var stickersDto = new StickersDtoForAI
            {
                Slot0Price = 10,
                Slot1Price = 20,
                Slot2Price = 30,
                Slot3Price = 40,
                StickerSlot1Name = "Sticker A",
                StickerSlot2Name = "Sticker B",
                StickerSlot3Name = "Sticker C",
                StickerSlot4Name = "Sticker D"
            };

            stickerMock
                .Setup(s => s.BuildStickersDtoForAiAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(stickersDto);

            promptFactoryMock
                .Setup(p => p.BuildChGunsPrompt(It.IsAny<AiChGunsRequest>()))
                .Returns("PROMPT_CH_GUN");

            llmMock
                .Setup(l => l.QueryAsync("PROMPT_CH_GUN", It.IsAny<string>()))
                .ReturnsAsync("EXPLANATION_CH_GUN");

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 1,
                Pattern = 123,
                FloatValue = 0.15,
                IsStattrak = false,
                PredictedPrice = 500,
                Stickers = new List<int> { 5, 6 }
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            var explanation = GetExplanation(result);
            explanation.Should().Be("EXPLANATION_CH_GUN");

            stickerMock.Verify(
                s => s.BuildStickersDtoForAiAsync(It.Is<IReadOnlyList<int>>(ids =>
                    ids.Count == 2 &&
                    ids.Contains(5) &&
                    ids.Contains(6))),
                Times.Once);
        }

        // ---------------------------------------------------------------------
        // LlmPriority.MiniThenGpt41 fallback на большую модель (fade_gun)
        // ---------------------------------------------------------------------

        [Fact]
        public async Task ExplainAsync_FadeGun_Uses_Fallback_When_Mini_Fails()
        {
            var options = CreateOptions();
            await using var db = new AppDbContext(options);

            var weapon = new Weapon { Id = 10, Name = "AK-47", WeaponTypeId = 1 };
            db.Weapons.Add(weapon);

            db.Skins.Add(new Skin
            {
                Id = 1,
                Name = "Fade",
                PatternStyle = "fade_gun",
                WeaponId = weapon.Id,
                Weapon = weapon
            });

            db.WearTiers.Add(new WearTier { Id = 1, Name = "Factory New" });
            db.SkinWearTiers.Add(new SkinWearTier
            {
                SkinId = 1,
                WearTierId = 1
            });

            db.FadeGunPatterns.Add(new FadeGunPattern
            {
                SkinId = 1,
                Pattern = 900,
                FadePercentage = 95,
                FadeRank = 1
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, out var stickerMock, out var promptFactoryMock, out var llmMock);

            var stickersDto = new StickersDtoForAI
            {
                Slot0Price = 1,
                Slot1Price = 2,
                Slot2Price = 3,
                Slot3Price = 4,
                StickerSlot1Name = "S1",
                StickerSlot2Name = "S2",
                StickerSlot3Name = "S3",
                StickerSlot4Name = "S4"
            };

            stickerMock
                .Setup(s => s.BuildStickersDtoForAiAsync(It.IsAny<IReadOnlyList<int>>()))
                .ReturnsAsync(stickersDto);

            promptFactoryMock
                .Setup(p => p.BuildFadeGunsPrompt(It.IsAny<AiFadeGunsRequest>()))
                .Returns("PROMPT_FADE_GUN");

            // Fallback логика: сначала mini падает, потом big успешен
            llmMock
                .Setup(l => l.QueryAsync("PROMPT_FADE_GUN", "gpt-4o-mini"))
                .ThrowsAsync(new Exception("mini failed"));

            llmMock
                .Setup(l => l.QueryAsync("PROMPT_FADE_GUN", "gpt-4.1-mini"))
                .ReturnsAsync("EXPLANATION_BIG");

            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 1,
                Pattern = 900,
                FloatValue = 0.01,
                IsStattrak = true,
                PredictedPrice = 999,
                Stickers = new List<int> { 1, 2 }
            };

            var result = await service.ExplainAsync(dto, LlmPriority.MiniThenGpt41);

            var explanation = GetExplanation(result);
            explanation.Should().Be("EXPLANATION_BIG");

            llmMock.Verify(l => l.QueryAsync("PROMPT_FADE_GUN", "gpt-4o-mini"), Times.Once);
            llmMock.Verify(l => l.QueryAsync("PROMPT_FADE_GUN", "gpt-4.1-mini"), Times.Once);
        }
    }
}
