using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin;
using cs2price_prediction.DTOs.Admin.WeaponTypes;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.WeaponTypes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin
{
    public class AdminWeaponTypesControllerTests
    {
        private readonly Mock<IAdminWeaponTypeService> _serviceMock;
        private readonly AdminWeaponTypesController _controller;

        public AdminWeaponTypesControllerTests()
        {
            _serviceMock = new Mock<IAdminWeaponTypeService>();
            _controller = new AdminWeaponTypesController(_serviceMock.Object);
        }

        // -------------------- CREATE --------------------

        [Fact]
        public async Task Create_Returns_Created_With_WeaponTypeDto()
        {
            // arrange
            var dto = new CreateWeaponTypeDto
            {
                Code = "rifle",
                Name = "Rifle"
            };

            var created = new WeaponTypeDto(
                Id: 10,
                Code: "rifle",
                Name: "Rifle"
            );

            _serviceMock
                .Setup(s => s.CreateWeaponTypeAsync(dto))
                .ReturnsAsync(created);

            // act
            var result = await _controller.Create(dto);

            // assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.ActionName.Should().Be(nameof(AdminWeaponTypesController.Create));
            createdResult.Value.Should().Be(created);

            _serviceMock.Verify(s => s.CreateWeaponTypeAsync(dto), Times.Once);
        }

        // -------------------- UPDATE --------------------

        [Fact]
        public async Task Update_Returns_Ok_When_Updated()
        {
            // arrange
            var dto = new UpdateWeaponTypeDto
            {
                Code = "pistol",
                Name = "Pistol"
            };

            var updated = new WeaponTypeDto(
                Id: 5,
                Code: "pistol",
                Name: "Pistol"
            );

            _serviceMock
                .Setup(s => s.UpdateWeaponTypeAsync(5, dto))
                .ReturnsAsync(updated);

            // act
            var result = await _controller.Update(5, dto);

            // assert
            var ok = result.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().Be(updated);

            _serviceMock.Verify(s => s.UpdateWeaponTypeAsync(5, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_Null()
        {
            // arrange
            var dto = new UpdateWeaponTypeDto
            {
                Code = "smg",
                Name = "SMG"
            };

            _serviceMock
                .Setup(s => s.UpdateWeaponTypeAsync(5, dto))
                .ReturnsAsync((WeaponTypeDto?)null);

            // act
            var result = await _controller.Update(5, dto);

            // assert
            result.Result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateWeaponTypeAsync(5, dto), Times.Once);
        }

        // -------------------- DELETE --------------------

        [Fact]
        public async Task Delete_Returns_NoContent_When_Deleted()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteWeaponTypeAsync(7))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteWeaponTypeAsync(7), Times.Once);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Not_Deleted()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteWeaponTypeAsync(7))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.DeleteWeaponTypeAsync(7), Times.Once);
        }
    }
}
