using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin.Patterns;
using cs2price_prediction.DTOs.Admin.Patterns.Doppler;
using cs2price_prediction.Services.Admin.Patterns.DopplerPhase;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin.Patterns
{
    public class AdminDopplerPhasesControllerTests
    {
        private readonly Mock<IAdminDopplerPhaseService> _serviceMock;
        private readonly AdminDopplerPhasesController _controller;

        public AdminDopplerPhasesControllerTests()
        {
            _serviceMock = new Mock<IAdminDopplerPhaseService>();
            _controller = new AdminDopplerPhasesController(_serviceMock.Object);
        }

        // ========== CREATE ==========

        [Fact]
        public async Task Create_Returns_Created_With_Id()
        {
            // arrange
            var dto = new CreateDopplerPhaseDto
            {
                Name = "Ruby"
            };

            _serviceMock
                .Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(42);

            // act
            var result = await _controller.Create(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminDopplerPhasesController.Create));
            created.Value.Should().Be(42);

            _serviceMock.Verify(s => s.CreateAsync(dto), Times.Once);
        }

        // ========== UPDATE ==========

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            var dto = new UpdateDopplerPhaseDto
            {
                Name = "Updated Ruby"
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
            var dto = new UpdateDopplerPhaseDto
            {
                Name = "Updated Ruby"
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
        public async Task Delete_Returns_Conflict_When_Service_Returns_False()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(7))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(7);

            // assert
            var conflict = result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("Phase not found or used by skins.");

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
