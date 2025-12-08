using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin;
using cs2price_prediction.DTOs.Admin.Stickers;
using cs2price_prediction.Services.Admin.Stickers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin
{
    public class AdminStickersControllerTests
    {
        private readonly Mock<IAdminStickerService> _serviceMock;
        private readonly AdminStickersController _controller;

        public AdminStickersControllerTests()
        {
            _serviceMock = new Mock<IAdminStickerService>();
            _controller = new AdminStickersController(_serviceMock.Object);
        }

        // -------------------- CREATE --------------------

        [Fact]
        public async Task CreateSticker_Returns_Created_With_Id()
        {
            // arrange
            var dto = new CreateStickerDto
            {
                Name = "Test Sticker",
                ReferencePrice = 12.34
            };

            _serviceMock
                .Setup(s => s.CreateStickerAsync(dto))
                .ReturnsAsync(42);

            // act
            var result = await _controller.CreateSticker(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminStickersController.CreateSticker));
            created.Value.Should().Be(42);

            _serviceMock.Verify(s => s.CreateStickerAsync(dto), Times.Once);
        }

        // -------------------- UPDATE --------------------

        [Fact]
        public async Task UpdateSticker_Returns_NoContent_When_Service_Returns_True()
        {
            // arrange
            var id = 10;
            var dto = new UpdateStickerDto
            {
                Name = "Updated Name",
                ReferencePrice = 20.0
            };

            _serviceMock
                .Setup(s => s.UpdateStickerAsync(id, dto))
                .ReturnsAsync(true);

            // act
            var result = await _controller.UpdateSticker(id, dto);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.UpdateStickerAsync(id, dto), Times.Once);
        }

        [Fact]
        public async Task UpdateSticker_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            var id = 10;
            var dto = new UpdateStickerDto
            {
                Name = "Updated Name",
                ReferencePrice = 20.0
            };

            _serviceMock
                .Setup(s => s.UpdateStickerAsync(id, dto))
                .ReturnsAsync(false);

            // act
            var result = await _controller.UpdateSticker(id, dto);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateStickerAsync(id, dto), Times.Once);
        }

        // -------------------- DELETE --------------------

        [Fact]
        public async Task DeleteSticker_Returns_NoContent_When_Service_Returns_True()
        {
            // arrange
            var id = 5;

            _serviceMock
                .Setup(s => s.DeleteStickerAsync(id))
                .ReturnsAsync(true);

            // act
            var result = await _controller.DeleteSticker(id);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteStickerAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteSticker_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            var id = 5;

            _serviceMock
                .Setup(s => s.DeleteStickerAsync(id))
                .ReturnsAsync(false);

            // act
            var result = await _controller.DeleteSticker(id);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.DeleteStickerAsync(id), Times.Once);
        }
    }
}
