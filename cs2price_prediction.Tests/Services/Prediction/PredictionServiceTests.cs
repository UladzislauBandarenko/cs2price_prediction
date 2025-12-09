using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.DTOs.Prediction;
using cs2price_prediction.DTOs.Ml;
using cs2price_prediction.Services.Prediction;
using cs2price_prediction.Services.Stickers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace cs2price_prediction.Tests.Services.Prediction
{
    public class PredictionServiceTests
    {
        #region Helpers

        private static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static PredictionService CreateService(
            AppDbContext db,
            HttpMessageHandler httpHandler)
        {
            var httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            var httpClientFactory = new FakeHttpClientFactory(httpClient);
            var stickerService = new FakeStickerService();

            return new PredictionService(db, httpClientFactory, stickerService);
        }

        private sealed class FakeHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;

            public FakeHttpClientFactory(HttpClient client)
            {
                _client = client;
            }

            public HttpClient CreateClient(string name) => _client;
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handle;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handle)
            {
                _handle = handle;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = _handle(request);
                return Task.FromResult(response);
            }
        }

        /// <summary>
        /// Простая реализация IStickerService, чтобы не тянуть Moq.
        /// ВАЖНО: сигнатура параметра должна совпадать с интерфейсом —
        /// IReadOnlyCollection&lt;int&gt;.
        /// </summary>
        private sealed class FakeStickerService : IStickerService
        {
            public Task<StickerFeatures> CalculateFeaturesAsync(IReadOnlyCollection<int> stickerIds)
            {
                // Для PredictionService в тестах нам не важны реальные значения,
                // поэтому возвращаем что-то простое и детерминированное.
                var features = new StickerFeatures
                {
                    StickersCount = stickerIds?.Count ?? 0,
                    StickersTotalValue = 0,
                    StickersAvgValue = 0,
                    StickersMaxValue = 0,
                    Slot0Price = 0,
                    Slot1Price = 0,
                    Slot2Price = 0,
                    Slot3Price = 0
                };

                return Task.FromResult(features);
            }
        }

        private static PredictionRequestDto CreateDefaultDto(
            int skinId = 1,
            int wearTierId = 1,
            double @float = 0.01,
            int pattern = 1,
            bool stattrak = false)
        {
            return new PredictionRequestDto
            {
                SkinId = skinId,
                WearTierId = wearTierId,
                FloatValue = @float,
                Pattern = pattern,
                IsStattrak = stattrak,
                Stickers = new List<int> { 1, 2, 3 }
            };
        }

        private static void SeedSkinWithWeaponAndWear(
            AppDbContext db,
            string patternStyle,
            int skinId = 1,
            int weaponId = 1,
            int wearTierId = 1)
        {
            var weapon = new Weapon
            {
                Id = weaponId,
                Name = "TestWeapon"
            };

            var skin = new Skin
            {
                Id = skinId,
                Name = "TestSkin",
                WeaponId = weaponId,
                Weapon = weapon,
                PatternStyle = patternStyle
            };

            var wear = new WearTier
            {
                Id = wearTierId,
                Name = "Factory New"
            };

            var link = new SkinWearTier
            {
                SkinId = skinId,
                WearTierId = wearTierId
            };

            db.Weapons.Add(weapon);
            db.Skins.Add(skin);
            db.WearTiers.Add(wear);
            db.SkinWearTiers.Add(link);
            db.SaveChanges();
        }

        #endregion

        [Fact]
        public async Task PredictAsync_Returns_NotFound_When_Skin_NotFound()
        {
            using var db = CreateInMemoryDbContext();

            var handler = new FakeHttpMessageHandler(_ =>
                throw new InvalidOperationException("HTTP client should not be called when skin is missing."));

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(skinId: 42);

            var result = await service.PredictAsync(dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Skin not found.", notFound.Value);
        }

        [Fact]
        public async Task PredictAsync_Returns_BadRequest_When_WearTier_NotFound()
        {
            using var db = CreateInMemoryDbContext();

            var weapon = new Weapon { Id = 1, Name = "AK-47" };
            var skin = new Skin
            {
                Id = 1,
                Name = "Redline",
                WeaponId = 1,
                Weapon = weapon,
                PatternStyle = "ch_knife"
            };

            db.Weapons.Add(weapon);
            db.Skins.Add(skin);
            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(_ =>
                throw new InvalidOperationException("HTTP client should not be called when wear tier is missing."));

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(skinId: 1, wearTierId: 999);

            var result = await service.PredictAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Wear tier not found.", badRequest.Value);
        }

        [Fact]
        public async Task PredictAsync_Returns_BadRequest_When_WearTier_NotAllowed_For_Skin()
        {
            using var db = CreateInMemoryDbContext();

            var weapon = new Weapon { Id = 1, Name = "AK-47" };
            var skin = new Skin
            {
                Id = 1,
                Name = "Redline",
                WeaponId = 1,
                Weapon = weapon,
                PatternStyle = "ch_knife"
            };

            var wear = new WearTier
            {
                Id = 10,
                Name = "Minimal Wear"
            };

            db.Weapons.Add(weapon);
            db.Skins.Add(skin);
            db.WearTiers.Add(wear);
            // НЕ добавляем SkinWearTier → wear не разрешён
            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(_ =>
                throw new InvalidOperationException("HTTP client should not be called when wear tier is not allowed."));

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(skinId: 1, wearTierId: 10);

            var result = await service.PredictAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("This wear tier is not available for the selected skin.", badRequest.Value);
        }

        [Fact]
        public async Task PredictAsync_Returns_BadRequest_For_Unsupported_PatternStyle()
        {
            using var db = CreateInMemoryDbContext();

            SeedSkinWithWeaponAndWear(db, patternStyle: "unknown_style");

            var handlerCalled = false;

            var handler = new FakeHttpMessageHandler(request =>
            {
                handlerCalled = true;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto();

            var result = await service.PredictAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unsupported pattern style: unknown_style", badRequest.Value);
            Assert.False(handlerCalled);
        }

        [Fact]
        public async Task PredictAsync_ChKnife_Calls_CaseHardened_Endpoint_And_Maps_Response()
        {
            using var db = CreateInMemoryDbContext();

            SeedSkinWithWeaponAndWear(db, patternStyle: "ch_knife");

            db.CaseHardenedKnifePatterns.Add(new CaseHardenedKnifePattern
            {
                Id = 1,
                SkinId = 1,
                Pattern = 1337,
                BacksideBlue = 50,
                BacksidePurple = 10,
                BacksideGold = 5,
                PlaysideBlue = 80,
                PlaysidePurple = 15,
                PlaysideGold = 0
            });

            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(request =>
            {
                Assert.Equal("/predict/case-hardened", request.RequestUri!.AbsolutePath);

                var mlResponse = new MlPredictionResponse
                {
                    PredictedPrice = 123.45
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(mlResponse)
                };
            });

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(pattern: 1337);

            var result = await service.PredictAsync(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<MlPredictionResponse>(ok.Value);

            Assert.Equal(123.45, payload.PredictedPrice);
        }

        [Fact]
        public async Task PredictAsync_ChKnife_Propagates_Ml_Error_Response()
        {
            using var db = CreateInMemoryDbContext();

            SeedSkinWithWeaponAndWear(db, patternStyle: "ch_knife");

            db.CaseHardenedKnifePatterns.Add(new CaseHardenedKnifePattern
            {
                Id = 1,
                SkinId = 1,
                Pattern = 777,
                BacksideBlue = 30,
                PlaysideBlue = 60
            });

            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(request =>
            {
                Assert.Equal("/predict/case-hardened", request.RequestUri!.AbsolutePath);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("ML error")
                };
            });

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(pattern: 777);

            var result = await service.PredictAsync(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("ML error", obj.Value);
        }

        [Fact]
        public async Task PredictAsync_FadeKnife_Uses_FadeKnives_Endpoint()
        {
            using var db = CreateInMemoryDbContext();

            SeedSkinWithWeaponAndWear(db, patternStyle: "fade_knife");

            db.FadeKnifePatterns.Add(new FadeKnifePattern
            {
                Id = 1,
                SkinId = 1,
                Pattern = 100,
                FadePercentage = 95,
                FadeRank = 1
            });

            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(request =>
            {
                Assert.Equal("/predict/fade-knives", request.RequestUri!.AbsolutePath);

                var mlResponse = new MlPredictionResponse
                {
                    PredictedPrice = 999.99
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(mlResponse)
                };
            });

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(pattern: 100);

            var result = await service.PredictAsync(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<MlPredictionResponse>(ok.Value);

            Assert.Equal(999.99, payload.PredictedPrice);
        }

        [Fact]
        public async Task PredictAsync_DopplerKnife_Uses_Doppler_Endpoint()
        {
            using var db = CreateInMemoryDbContext();

            SeedSkinWithWeaponAndWear(db, patternStyle: "doppler_knife");

            var phase = new DopplerPhase
            {
                Id = 5,
                Name = "Ruby"
            };

            var dopplerLink = new DopplerSkinPhase
            {
                Id = 1,
                SkinId = 1,
                PhaseId = phase.Id,
                Phase = phase
            };

            db.DopplerPhases.Add(phase);
            db.DopplerSkinPhases.Add(dopplerLink);
            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(request =>
            {
                Assert.Equal("/predict/doppler", request.RequestUri!.AbsolutePath);

                var mlResponse = new MlPredictionResponse
                {
                    PredictedPrice = 5000.0
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(mlResponse)
                };
            });

            var service = CreateService(db, handler);

            var dto = CreateDefaultDto(pattern: phase.Id);

            var result = await service.PredictAsync(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<MlPredictionResponse>(ok.Value);

            Assert.Equal(5000.0, payload.PredictedPrice);
        }

        [Fact]
        public async Task PredictAsync_DopplerKnife_Returns_BadRequest_When_Phase_Not_Found()
        {
            using var db = CreateInMemoryDbContext();

            SeedSkinWithWeaponAndWear(db, patternStyle: "doppler_knife");
            db.SaveChanges();

            var handler = new FakeHttpMessageHandler(_ =>
            {
                throw new InvalidOperationException("HTTP client should not be called when doppler phase is missing.");
            });

            var service = CreateService(db, handler);
            var dto = CreateDefaultDto(pattern: 999);

            var result = await service.PredictAsync(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Doppler phase not found for this skin (Pattern as PhaseId).", badRequest.Value);
        }
    }
}
