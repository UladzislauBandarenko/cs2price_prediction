using System;
using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.DopplerSkin;
using cs2price_prediction.Services.Admin.Patterns.DopplerSkinPhase;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin.Patterns
{
    public class AdminDopplerSkinPhasesControllerTests
    {
        private readonly Mock<IAdminDopplerSkinPhaseService> _serviceMock;
        private readonly AdminDopplerSkinPhasesController _controller;

        public AdminDopplerSkinPhasesControllerTests()
        {
            _serviceMock = new Mock<IAdminDopplerSkinPhaseService>();
            _controller = new AdminDopplerSkinPhasesController(_serviceMock.Object);
        }

        // ========== CREATE ==========

        [Fact]
        public async Task Create_Returns_Created_When_Service_Succeeds()
        {
            // arrange
            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 1,
                PhaseId = 2
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(42);

            // act
            var result = await _controller.Create(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminDopplerSkinPhasesController.Create));
            created.Value.Should().Be(42);

            _serviceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Create_Returns_Conflict_When_Service_Throws_Exception()
        {
            // arrange
            var dto = new CreateDopplerSkinPhaseDto
            {
                SkinId = 1,
                PhaseId = 2
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
            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 3
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
            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 3
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

        [Fact]
        public async Task Update_Returns_Conflict_When_Service_Throws_Exception()
        {
            // arrange
            var dto = new UpdateDopplerSkinPhaseDto
            {
                PhaseId = 3
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(10, dto))
                .ThrowsAsync(new InvalidOperationException("update error"));

            // act
            var result = await _controller.Update(10, dto);

            // assert
            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("update error");

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
