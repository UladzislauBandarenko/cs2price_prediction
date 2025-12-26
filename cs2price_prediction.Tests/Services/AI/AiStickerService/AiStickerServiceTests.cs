using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Stickers;
using cs2price_prediction.DTOs.AI.StickersDtoForAI;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

// Alias to explicitly reference the service class and avoid confusion with the namespace
using AiStickerServiceImpl = cs2price_prediction.Services.AI.AiStickerService.AiStickerService;

namespace cs2price_prediction.Tests.Services.AI.AiStickerService
{
    public class AiStickerServiceTests
    {
        private static DbContextOptions<AppDbContext> CreateOptions()
            => new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private static AiStickerServiceImpl CreateService(DbContextOptions<AppDbContext> options)
            => new AiStickerServiceImpl(new AppDbContext(options));

        private static void AssertDefaultDto(StickersDtoForAI dto)
        {
            dto.Slot0Price.Should().Be(0);
            dto.Slot1Price.Should().Be(0);
            dto.Slot2Price.Should().Be(0);
            dto.Slot3Price.Should().Be(0);

            dto.StickerSlot1Name.Should().BeNull();
            dto.StickerSlot2Name.Should().BeNull();
            dto.StickerSlot3Name.Should().BeNull();
            dto.StickerSlot4Name.Should().BeNull();
        }

        // -----------------------------
        // Basic cases
        // -----------------------------

        [Fact]
        public async Task BuildStickersDtoForAiAsync_Returns_Default_When_Ids_Empty()
        {
            var options = CreateOptions();
            var service = CreateService(options);

            var result = await service.BuildStickersDtoForAiAsync(Array.Empty<int>());

            AssertDefaultDto(result);
        }

        [Fact]
        public async Task BuildStickersDtoForAiAsync_Returns_Default_When_Ids_All_NonPositive()
        {
            var options = CreateOptions();
            var service = CreateService(options);

            IReadOnlyList<int> ids = new[] { 0, -1, -5 };

            var result = await service.BuildStickersDtoForAiAsync(ids);

            AssertDefaultDto(result);
        }

        [Fact]
        public async Task BuildStickersDtoForAiAsync_Returns_Default_When_Ids_Not_Found_In_Db()
        {
            var options = CreateOptions();

            await using (var seed = new AppDbContext(options))
            {
                // The database is empty, no stickers are created
                await seed.SaveChangesAsync();
            }

            var service = CreateService(options);
            IReadOnlyList<int> ids = new[] { 1, 2, 3 };

            var result = await service.BuildStickersDtoForAiAsync(ids);

            AssertDefaultDto(result);
        }

        // -----------------------------
        // Normal case: multiple stickers, multiple prices
        // -----------------------------

        [Fact]
        public async Task BuildStickersDtoForAiAsync_Fills_Slots_In_Order_Using_Latest_Prices()
        {
            var options = CreateOptions();

            await using (var seed = new AppDbContext(options))
            {
                var s1 = new Sticker { Id = 1, Name = "Sticker A" };
                var s2 = new Sticker { Id = 2, Name = "Sticker B" };
                var s3 = new Sticker { Id = 3, Name = "Sticker C" };
                var s4 = new Sticker { Id = 4, Name = "Sticker D" };

                seed.Stickers.AddRange(s1, s2, s3, s4);

                // For each sticker, create multiple prices to verify
                // that the latest one is used (by Id, since OrderByDescending(p.Id))
                seed.StickerPrices.AddRange(
                    new StickerPrice { Id = 1, Sticker = s1, Price = 1.0 },
                    new StickerPrice { Id = 2, Sticker = s1, Price = 1.5 },   // latest

                    new StickerPrice { Id = 3, Sticker = s2, Price = 2.0 },
                    new StickerPrice { Id = 4, Sticker = s2, Price = 2.2 },   // latest

                    new StickerPrice { Id = 5, Sticker = s3, Price = 3.0 },   // only one
                    new StickerPrice { Id = 6, Sticker = s4, Price = 4.4 }    // only one
                );

                await seed.SaveChangesAsync();
            }

            var service = CreateService(options);

            // stickerIds by slots: 0→1, 1→2, 2→3, 3→4
            IReadOnlyList<int> ids = new[] { 1, 2, 3, 4 };

            var result = await service.BuildStickersDtoForAiAsync(ids);

            result.Slot0Price.Should().Be(1.5);   // latest price for Sticker 1
            result.Slot1Price.Should().Be(2.2);   // latest price for Sticker 2
            result.Slot2Price.Should().Be(3.0);
            result.Slot3Price.Should().Be(4.4);

            result.StickerSlot1Name.Should().Be("Sticker A");
            result.StickerSlot2Name.Should().Be("Sticker B");
            result.StickerSlot3Name.Should().Be("Sticker C");
            result.StickerSlot4Name.Should().Be("Sticker D");
        }

        // -----------------------------
        // Ignoring invalid / duplicate values
        // -----------------------------

        [Fact]
        public async Task BuildStickersDtoForAiAsync_Ignores_Invalid_And_Unknown_Ids()
        {
            var options = CreateOptions();

            await using (var seed = new AppDbContext(options))
            {
                var s1 = new Sticker { Id = 10, Name = "Sticker X" };
                var s2 = new Sticker { Id = 20, Name = "Sticker Y" };

                seed.Stickers.AddRange(s1, s2);

                seed.StickerPrices.AddRange(
                    new StickerPrice { Id = 1, Sticker = s1, Price = 10.0 },
                    new StickerPrice { Id = 2, Sticker = s2, Price = 20.0 }
                );

                await seed.SaveChangesAsync();
            }

            var service = CreateService(options);

            // Mixed input: non-existing id (999), zero, negative, and two valid ones
            IReadOnlyList<int> ids = new[] { 999, 0, -5, 10, 20 };

            var result = await service.BuildStickersDtoForAiAsync(ids);

            // FillSlot(0,0) → 999 → not in dict → skipped
            // FillSlot(1,1) → 0   → <=0        → skipped
            // FillSlot(2,2) → -5  → <=0        → skipped
            // FillSlot(3,3) → 10  → ok → Slot3 + StickerSlot4Name

            result.Slot0Price.Should().Be(0);
            result.Slot1Price.Should().Be(0);
            result.Slot2Price.Should().Be(0);
            result.Slot3Price.Should().Be(10.0);

            result.StickerSlot1Name.Should().BeNull();
            result.StickerSlot2Name.Should().BeNull();
            result.StickerSlot3Name.Should().BeNull();
            result.StickerSlot4Name.Should().Be("Sticker X");
        }
    }
}
