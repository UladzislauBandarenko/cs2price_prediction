using System;
using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin;
using cs2price_prediction.DTOs.Admin.SkinWearTiers;
using cs2price_prediction.Services.Admin.SkinWearTiers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin
{
    public class AdminSkinWearTiersControllerTests
    {
        private readonly Mock<IAdminSkinWearTierService> _serviceMock;
        private readonly AdminSkinWearTiersController _controller;

        public AdminSkinWearTiersControllerTests()
        {
            _serviceMock = new Mock<IAdminSkinWearTierService>();
            _controller = new AdminSkinWearTiersController(_serviceMock.Object);
        }

        // -------------------- CREATE --------------------

        [Fact]
        public async Task Create_Returns_NoContent_When_Created()
        {
            var dto = new CreateSkinWearTierDto { SkinId = 1, WearTierId = 2 };

            _serviceMock
                .Setup(s => s.CreateSkinWearTierAsync(dto))
                .ReturnsAsync(true);

            var result = await _controller.Create(dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Create_Returns_Conflict_When_AlreadyExists()
        {
            var dto = new CreateSkinWearTierDto { SkinId = 1, WearTierId = 2 };

            _serviceMock
                .Setup(s => s.CreateSkinWearTierAsync(dto))
                .ReturnsAsync(false);

            var result = await _controller.Create(dto);

            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("SkinWearTier already exists.");
        }

        [Fact]
        public async Task Create_Returns_BadRequest_On_ArgumentException()
        {
            var dto = new CreateSkinWearTierDto { SkinId = 1, WearTierId = 2 };

            _serviceMock
                .Setup(s => s.CreateSkinWearTierAsync(dto))
                .ThrowsAsync(new ArgumentException("Skin not found"));

            var result = await _controller.Create(dto);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.Value.Should().Be("Skin not found");
        }

        // -------------------- UPDATE --------------------

        [Fact]
        public async Task Update_Returns_NoContent_When_Updated()
        {
            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 3
            };

            _serviceMock
                .Setup(s => s.UpdateSkinWearTierAsync(dto))
                .ReturnsAsync(true);

            var result = await _controller.Update(dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_False()
        {
            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 3
            };

            _serviceMock
                .Setup(s => s.UpdateSkinWearTierAsync(dto))
                .ReturnsAsync(false);

            var result = await _controller.Update(dto);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_Returns_BadRequest_On_ArgumentException()
        {
            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 3
            };

            _serviceMock
                .Setup(s => s.UpdateSkinWearTierAsync(dto))
                .ThrowsAsync(new ArgumentException("WearTier not found"));

            var result = await _controller.Update(dto);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.Value.Should().Be("WearTier not found");
        }

        [Fact]
        public async Task Update_Returns_Conflict_On_InvalidOperationException()
        {
            var dto = new UpdateSkinWearTierDto
            {
                SkinId = 1,
                OldWearTierId = 2,
                NewWearTierId = 3
            };

            _serviceMock
                .Setup(s => s.UpdateSkinWearTierAsync(dto))
                .ThrowsAsync(new InvalidOperationException("Duplicate tier"));

            var result = await _controller.Update(dto);

            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("Duplicate tier");
        }

        // -------------------- DELETE --------------------

        [Fact]
        public async Task Delete_Returns_NoContent_When_Deleted()
        {
            var dto = new DeleteSkinWearTierDto { SkinId = 1, WearTierId = 2 };

            _serviceMock
                .Setup(s => s.DeleteSkinWearTierAsync(1, 2))
                .ReturnsAsync(true);

            var result = await _controller.Delete(dto);

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Not_Deleted()
        {
            var dto = new DeleteSkinWearTierDto { SkinId = 1, WearTierId = 2 };

            _serviceMock
                .Setup(s => s.DeleteSkinWearTierAsync(1, 2))
                .ReturnsAsync(false);

            var result = await _controller.Delete(dto);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
