using cs2price_prediction.Data;
using cs2price_prediction.Domain.Stickers;
using cs2price_prediction.Services.Stickers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace cs2price_prediction.Tests.Services.Stickers
{
    public class StickerServiceTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CalculateFeaturesAsync_Returns_Default_When_Ids_Empty()
        {
            // arrange
            using var db = CreateDbContext(Guid.NewGuid().ToString());
            var service = new StickerService(db);

            var ids = Array.Empty<int>();

            // act
            var result = await service.CalculateFeaturesAsync(ids);

            // assert
            result.StickersCount.Should().Be(0);
            result.StickersTotalValue.Should().Be(0);
            result.StickersAvgValue.Should().Be(0);
            result.StickersMaxValue.Should().Be(0);
            result.Slot0Price.Should().Be(0);
            result.Slot1Price.Should().Be(0);
            result.Slot2Price.Should().Be(0);
            result.Slot3Price.Should().Be(0);
        }

        [Fact]
        public async Task CalculateFeaturesAsync_Returns_Default_When_Prices_Not_Found()
        {
            // arrange
            using var db = CreateDbContext(Guid.NewGuid().ToString());
            var service = new StickerService(db);

            // в БД нет StickerPrices с такими Id
            var ids = new[] { 1, 2, 3 };

            // act
            var result = await service.CalculateFeaturesAsync(ids);

            // assert
            result.StickersCount.Should().Be(0);
            result.StickersTotalValue.Should().Be(0);
            result.StickersAvgValue.Should().Be(0);
            result.StickersMaxValue.Should().Be(0);
        }

        [Fact]
        public async Task CalculateFeaturesAsync_Computes_Aggregates_And_Slots()
        {
            // arrange
            using var db = CreateDbContext(Guid.NewGuid().ToString());

            // ⚠️ здесь я предполагаю, что у тебя есть сущность StickerPrice
            // с полями StickerId (int) и Price (double/decimal).
            // Если название/типы другие — просто подправь этот участок под свою модель.
            db.StickerPrices.AddRange(
                new StickerPrice { StickerId = 1, Price = 10.0 },
                new StickerPrice { StickerId = 2, Price = 20.0 },
                new StickerPrice { StickerId = 3, Price = 5.0 }
            );
            await db.SaveChangesAsync();

            var service = new StickerService(db);

            var ids = new[] { 1, 2, 3 };

            // act
            var result = await service.CalculateFeaturesAsync(ids);

            // assert
            result.StickersCount.Should().Be(3);
            result.StickersTotalValue.Should().Be(35.0);   // 10 + 20 + 5
            result.StickersAvgValue.Should().BeApproximately(35.0 / 3.0, 0.0001);
            result.StickersMaxValue.Should().Be(20.0);

            // порядок зависит от того, как EF вернёт список.
            // В твоём коде: prices = ... Select(p => p.Price).ToListAsync();
            // Обычно он соответствует порядку добавления.
            result.Slot0Price.Should().Be(10.0);
            result.Slot1Price.Should().Be(20.0);
            result.Slot2Price.Should().Be(5.0);
            result.Slot3Price.Should().Be(0.0); // ElementAtOrDefault(3) → 0
        }
    }
}