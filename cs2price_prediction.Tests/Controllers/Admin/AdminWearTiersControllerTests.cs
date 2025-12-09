using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin;
using cs2price_prediction.DTOs.Admin.WearTiers;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.WearTiers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin
{
    public class AdminWearTiersControllerTests
    {
        private readonly Mock<IAdminWearTierService> _serviceMock;
        private readonly AdminWearTiersController _controller;

        public AdminWearTiersControllerTests()
        {
            _serviceMock = new Mock<IAdminWearTierService>();
            _controller = new AdminWearTiersController(_serviceMock.Object);
        }

        // ========= CREATE =========

        [Fact]
        public async Task Create_Returns_Created_With_WearTierDto()
        {
            // arrange
            var dto = new CreateWearTierDto
            {
                Name = "Field-Tested"
            };

            var createdDto = new WearTierDto(
                Id: 5,
                Name: "Field-Tested"
            );

            _serviceMock
                .Setup(s => s.CreateWearTierAsync(dto))
                .ReturnsAsync(createdDto);

            // act
            var result = await _controller.Create(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminWearTiersController.Create));
            created.Value.Should().Be(createdDto);

            _serviceMock.Verify(s => s.CreateWearTierAsync(dto), Times.Once);
        }

        // ========= UPDATE =========

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_Null()
        {
            // arrange
            var dto = new UpdateWearTierDto
            {
                Name = "New Name"
            };

            _serviceMock
                .Setup(s => s.UpdateWearTierAsync(10, dto))
                .ReturnsAsync((WearTierDto?)null);

            // act
            var result = await _controller.Update(10, dto);

            // assert
            result.Result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateWearTierAsync(10, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Returns_Ok_With_Updated_Dto()
        {
            // arrange
            var dto = new UpdateWearTierDto
            {
                Name = "Battle-Scarred"
            };

            var updatedDto = new WearTierDto(
                Id: 3,
                Name: "Battle-Scarred"
            );

            _serviceMock
                .Setup(s => s.UpdateWearTierAsync(3, dto))
                .ReturnsAsync(updatedDto);

            // act
            var result = await _controller.Update(3, dto);

            // assert
            var ok = result.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().Be(updatedDto);

            _serviceMock.Verify(s => s.UpdateWearTierAsync(3, dto), Times.Once);
        }

        // ========= DELETE =========

        [Fact]
        public async Task Delete_Returns_NoContent_When_Deleted()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteWearTierAsync(7))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteWearTierAsync(7), Times.Once);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Not_Deleted()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteWearTierAsync(7))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.DeleteWearTierAsync(7), Times.Once);
        }
    }
}
