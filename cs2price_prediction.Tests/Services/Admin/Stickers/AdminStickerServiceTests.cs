using System;
using System.Linq;
using System.Threading.Tasks;
using cs2price_prediction.Data;
using cs2price_prediction.Domain.Stickers;
using cs2price_prediction.DTOs.Admin.Stickers;
using cs2price_prediction.Services.Admin.Stickers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.Admin.Stickers
{
    public class AdminStickerServiceTests
    {
        private (AdminStickerService service, DbContextOptions<AppDbContext> options) CreateService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var factoryMock = new Mock<IAdminDbContextFactory>();
            factoryMock
                .Setup(f => f.CreateAdminContext())
                .Returns(() => new AppDbContext(options));

            var service = new AdminStickerService(factoryMock.Object);
            return (service, options);
        }

        // ----------------- CreateStickerAsync -----------------

        [Fact]
        public async Task CreateStickerAsync_Creates_Sticker_Without_Price_When_ReferencePrice_Null()
        {
            // arrange
            var (service, options) = CreateService();

            var dto = new CreateStickerDto
            {
                Name = "  Test Sticker  ",
                ReferencePrice = null
            };

            // act
            int id = await service.CreateStickerAsync(dto);

            // assert
            id.Should().BeGreaterThan(0);

            await using var db = new AppDbContext(options);
            var sticker = await db.Stickers.FirstOrDefaultAsync(s => s.Id == id);
            sticker.Should().NotBeNull();
            sticker!.Name.Should().Be("Test Sticker");

            var prices = await db.StickerPrices.Where(p => p.StickerId == id).ToListAsync();
            prices.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateStickerAsync_Creates_Sticker_And_Price_When_ReferencePrice_Provided()
        {
            // arrange
            var (service, options) = CreateService();

            var dto = new CreateStickerDto
            {
                Name = "  Holo Sticker  ",
                // int подойдёт и для double?, и для decimal?, и для float?
                ReferencePrice = 100
            };

            // act
            int id = await service.CreateStickerAsync(dto);

            // assert
            await using var db = new AppDbContext(options);
            var sticker = await db.Stickers.FirstOrDefaultAsync(s => s.Id == id);
            sticker.Should().NotBeNull();
            sticker!.Name.Should().Be("Holo Sticker");

            var prices = await db.StickerPrices.Where(p => p.StickerId == id).ToListAsync();
            prices.Should().HaveCount(1);
            prices[0].Price.Should().Be(100);
        }

        // ----------------- UpdateStickerAsync -----------------

        [Fact]
        public async Task UpdateStickerAsync_ReturnsFalse_When_Sticker_Not_Found()
        {
            // arrange
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            var dto = new UpdateStickerDto
            {
                Name = "New Name",
                ReferencePrice = null
            };

            // act
            bool updated = await service.UpdateStickerAsync(999, dto);

            // assert
            updated.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateStickerAsync_Updates_Name_Without_Adding_Price_When_ReferencePrice_Null()
        {
            // arrange
            var (service, options) = CreateService();
            int id;

            await using (var seed = new AppDbContext(options))
            {
                var s = new Sticker { Name = "Old Name" };
                seed.Stickers.Add(s);
                await seed.SaveChangesAsync();
                id = s.Id;
            }

            var dto = new UpdateStickerDto
            {
                Name = "  New Name  ",
                ReferencePrice = null
            };

            // act
            bool updated = await service.UpdateStickerAsync(id, dto);

            // assert
            updated.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var sticker = await db.Stickers.FirstOrDefaultAsync(s => s.Id == id);
            sticker.Should().NotBeNull();
            sticker!.Name.Should().Be("New Name");

            var prices = await db.StickerPrices.Where(p => p.StickerId == id).ToListAsync();
            prices.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateStickerAsync_Adds_New_Price_When_ReferencePrice_Provided()
        {
            // arrange
            var (service, options) = CreateService();
            int id;

            await using (var seed = new AppDbContext(options))
            {
                var s = new Sticker { Name = "Sticker With Price" };
                seed.Stickers.Add(s);
                await seed.SaveChangesAsync();
                id = s.Id;
            }

            var dto = new UpdateStickerDto
            {
                Name = "Updated Sticker",
                ReferencePrice = 250
            };

            // act
            bool updated = await service.UpdateStickerAsync(id, dto);

            // assert
            updated.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var sticker = await db.Stickers.FirstOrDefaultAsync(s => s.Id == id);
            sticker.Should().NotBeNull();
            sticker!.Name.Should().Be("Updated Sticker");

            var prices = await db.StickerPrices.Where(p => p.StickerId == id).ToListAsync();
            prices.Should().HaveCount(1);
            prices[0].Price.Should().Be(250);
        }

        // ----------------- DeleteStickerAsync -----------------

        [Fact]
        public async Task DeleteStickerAsync_ReturnsFalse_When_Not_Found()
        {
            // arrange
            var (service, options) = CreateService();

            await using (var seed = new AppDbContext(options))
            {
                await seed.SaveChangesAsync();
            }

            // act
            bool deleted = await service.DeleteStickerAsync(123);

            // assert
            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteStickerAsync_Removes_Sticker_And_ReturnsTrue()
        {
            // arrange
            var (service, options) = CreateService();
            int id;

            await using (var seed = new AppDbContext(options))
            {
                var s = new Sticker { Name = "To Delete" };
                seed.Stickers.Add(s);
                await seed.SaveChangesAsync();
                id = s.Id;
            }

            // act
            bool deleted = await service.DeleteStickerAsync(id);

            // assert
            deleted.Should().BeTrue();

            await using var db = new AppDbContext(options);
            var sticker = await db.Stickers.FirstOrDefaultAsync(s => s.Id == id);
            sticker.Should().BeNull();
        }
    }
}
