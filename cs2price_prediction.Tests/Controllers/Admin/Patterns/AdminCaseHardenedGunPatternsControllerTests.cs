using System;
using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.CaseHardenedGun;
using cs2price_prediction.Services.Admin.Patterns.CaseHardenedGun;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin.Patterns
{
    public class AdminCaseHardenedGunPatternsControllerTests
    {
        private readonly Mock<IAdminCaseHardenedGunPatternService> _serviceMock;
        private readonly AdminCaseHardenedGunPatternsController _controller;

        public AdminCaseHardenedGunPatternsControllerTests()
        {
            _serviceMock = new Mock<IAdminCaseHardenedGunPatternService>();
            _controller = new AdminCaseHardenedGunPatternsController(_serviceMock.Object);
        }

        // ========== CREATE ==========

        [Fact]
        public async Task Create_Returns_Created_When_Service_Returns_Id()
        {
            // arrange
            var dto = new CreateCaseHardenedGunPatternDto
            {
                SkinId = 10,
                Pattern = 123,
                PlaysideBlue = 40,
                BacksideBlue = 30
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(42);

            // act
            var result = await _controller.Create(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminCaseHardenedGunPatternsController.Create));
            created.Value.Should().Be(42);

            _serviceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Create_Returns_Conflict_When_Service_Throws_Exception()
        {
            // arrange
            var dto = new CreateCaseHardenedGunPatternDto
            {
                SkinId = 10,
                Pattern = 123,
                PlaysideBlue = 40,
                BacksideBlue = 30
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ThrowsAsync(new InvalidOperationException("Pattern already exists"));

            // act
            var result = await _controller.Create(dto);

            // assert
            var conflict = result.Result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("Pattern already exists");

            _serviceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        // ========== UPDATE ==========

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            var dto = new UpdateCaseHardenedGunPatternDto
            {
                PlaysideBlue = 50,
                BacksideBlue = 45
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(5, dto))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Update(5, dto);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateAsync(5, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Returns_NoContent_When_Service_Returns_True()
        {
            // arrange
            var dto = new UpdateCaseHardenedGunPatternDto
            {
                PlaysideBlue = 50,
                BacksideBlue = 45
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(5, dto))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Update(5, dto);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.UpdateAsync(5, dto), Times.Once);
        }

        // ========== DELETE ==========

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
    }
}
