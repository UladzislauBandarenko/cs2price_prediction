using System;
using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin;
using cs2price_prediction.DTOs.Admin.Weapons;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.Weapons;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin
{
    public class AdminWeaponsControllerTests
    {
        private readonly Mock<IAdminWeaponService> _serviceMock;
        private readonly AdminWeaponsController _controller;

        public AdminWeaponsControllerTests()
        {
            _serviceMock = new Mock<IAdminWeaponService>();
            _controller = new AdminWeaponsController(_serviceMock.Object);
        }

        // ============ CREATE ============

        [Fact]
        public async Task Create_Returns_Created_When_Service_Returns_Weapon()
        {
            // arrange
            var dto = new CreateWeaponDto
            {
                Name = "AK-47",
                WeaponTypeId = 1
            };

            // WeaponDto из DTOs.Meta:
            // record WeaponDto(int Id, string Name)
            var createdWeapon = new WeaponDto(10, "AK-47");

            _serviceMock
                .Setup(s => s.CreateWeaponAsync(dto))
                .ReturnsAsync(createdWeapon);

            // act
            var result = await _controller.Create(dto);

            // assert
            var created = result.Result as CreatedAtActionResult;
            created.Should().NotBeNull();
            created!.ActionName.Should().Be(nameof(AdminWeaponsController.Create));
            created.Value.Should().Be(createdWeapon);

            _serviceMock.Verify(s => s.CreateWeaponAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Service_Throws_ArgumentException()
        {
            // arrange
            var dto = new CreateWeaponDto
            {
                Name = "AK-47",
                WeaponTypeId = 1
            };

            _serviceMock
                .Setup(s => s.CreateWeaponAsync(dto))
                .ThrowsAsync(new ArgumentException("Bad weapon type"));

            // act
            var result = await _controller.Create(dto);

            // assert
            var bad = result.Result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.Value.Should().Be("Bad weapon type");

            _serviceMock.Verify(s => s.CreateWeaponAsync(dto), Times.Once);
        }

        // ============ UPDATE ============

        [Fact]
        public async Task Update_Returns_Ok_When_Weapon_Updated()
        {
            // arrange
            var dto = new UpdateWeaponDto
            {
                Name = "New Name",
                WeaponTypeId = 2
            };

            var updatedWeapon = new WeaponDto(5, "New Name");

            _serviceMock
                .Setup(s => s.UpdateWeaponAsync(5, dto))
                .ReturnsAsync(updatedWeapon);

            // act
            var result = await _controller.Update(5, dto);

            // assert
            var ok = result.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().Be(updatedWeapon);

            _serviceMock.Verify(s => s.UpdateWeaponAsync(5, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_Null()
        {
            // arrange
            var dto = new UpdateWeaponDto
            {
                Name = "New Name",
                WeaponTypeId = 2
            };

            _serviceMock
                .Setup(s => s.UpdateWeaponAsync(5, dto))
                .ReturnsAsync((WeaponDto?)null);

            // act
            var result = await _controller.Update(5, dto);

            // assert
            result.Result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateWeaponAsync(5, dto), Times.Once);
        }

        [Fact]
        public async Task Update_Returns_BadRequest_When_Service_Throws_ArgumentException()
        {
            // arrange
            var dto = new UpdateWeaponDto
            {
                Name = "New Name",
                WeaponTypeId = 2
            };

            _serviceMock
                .Setup(s => s.UpdateWeaponAsync(5, dto))
                .ThrowsAsync(new ArgumentException("Invalid weapon type"));

            // act
            var result = await _controller.Update(5, dto);

            // assert
            var bad = result.Result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.Value.Should().Be("Invalid weapon type");

            _serviceMock.Verify(s => s.UpdateWeaponAsync(5, dto), Times.Once);
        }

        // ============ DELETE ============

        [Fact]
        public async Task Delete_Returns_NoContent_When_Deleted()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteWeaponAsync(7))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteWeaponAsync(7), Times.Once);
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Not_Deleted()
        {
            // arrange
            _serviceMock
                .Setup(s => s.DeleteWeaponAsync(7))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(7);

            // assert
            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.DeleteWeaponAsync(7), Times.Once);
        }
    }
}
