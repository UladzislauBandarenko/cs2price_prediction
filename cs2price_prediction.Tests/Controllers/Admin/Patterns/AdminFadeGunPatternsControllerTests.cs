using System;
using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.FadeGun;
using cs2price_prediction.Services.Admin.Patterns.FadeGun;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin.Patterns
{
    public class AdminFadeGunPatternsControllerTests
    {
        private readonly Mock<IAdminFadeGunPatternService> _serviceMock;
        private readonly AdminFadeGunPatternsController _controller;

        public AdminFadeGunPatternsControllerTests()
        {
            _serviceMock = new Mock<IAdminFadeGunPatternService>();
            _controller = new AdminFadeGunPatternsController(_serviceMock.Object);
        }

        // ========== CREATE ==========

        [Fact]
        public async Task Create_Returns_Created_When_Service_Succeeds()
        {
            // arrange
            var dto = new CreateFadeGunPatternDto
            {
                SkinId = 1,
                Pattern = 123,
                FadePercentage = 95.5,
                FadeRank = 1
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(42);

            // act
            var result = await _controller.Create(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminFadeGunPatternsController.Create));
            created.Value.Should().Be(42);

            _serviceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Create_Returns_Conflict_When_Service_Throws_Exception()
        {
            // arrange
            var dto = new CreateFadeGunPatternDto
            {
                SkinId = 1,
                Pattern = 123,
                FadePercentage = 95.5,
                FadeRank = 1
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ThrowsAsync(new InvalidOperationException("some error"));

            // act
            var result = await _controller.Create(dto);

            // assert
            var conflict = result.Result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("some error");

            _serviceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        // ========== UPDATE ==========

        [Fact]
        public async Task Update_Returns_NoContent_When_Service_Returns_True()
        {
            // arrange
            var dto = new UpdateFadeGunPatternDto
            {
                FadePercentage = 90.0,
                FadeRank = 2
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(10, dto))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Update(10, dto);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.UpdateAsync(10, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            var dto = new UpdateFadeGunPatternDto
            {
                FadePercentage = 90.0,
                FadeRank = 2
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(10, dto))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Update(10, dto);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateAsync(10, dto), Times.Once);
        }

        // ========== DELETE ==========

        [Fact]
        public async Task Delete_Returns_NoContent_When_Service_Returns_True()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(7))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteAsync(7), Times.Once);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(7))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.DeleteAsync(7), Times.Once);
        }
    }
}
