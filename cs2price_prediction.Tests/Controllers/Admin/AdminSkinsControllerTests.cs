using System;
using System.Threading.Tasks;
using cs2price_prediction.Controllers.Admin;
using cs2price_prediction.DTOs.Admin.Skins;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Admin.Skins;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Admin
{
    public class AdminSkinsControllerTests
    {
        private readonly Mock<IAdminSkinService> _serviceMock;
        private readonly AdminSkinsController _controller;

        public AdminSkinsControllerTests()
        {
            _serviceMock = new Mock<IAdminSkinService>();
            _controller = new AdminSkinsController(_serviceMock.Object);
        }

        // ---------- CREATE ----------

        [Fact]
        public async Task Create_Returns_Created_When_Service_Succeeds()
        {
            // arrange
            var dto = new CreateSkinDto
            {
                WeaponId = 1,
                Name = "Test Skin",
                PatternStyle = "ch_gun"
            };

            var created = new SkinDto(10, "Test Skin", "ch_gun");

            _serviceMock
                .Setup(s => s.CreateSkinAsync(dto))
                .ReturnsAsync(created);

            // act
            var result = await _controller.Create(dto);

            // assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.ActionName.Should().Be(nameof(AdminSkinsController.Create));
            createdResult.Value.Should().BeEquivalentTo(created);
        }

        [Fact]
        public async Task Create_Returns_BadRequest_When_Service_Throws_ArgumentException()
        {
            // arrange
            var dto = new CreateSkinDto
            {
                WeaponId = 999,
                Name = "Invalid",
                PatternStyle = "ch_gun"
            };

            _serviceMock
                .Setup(s => s.CreateSkinAsync(dto))
                .ThrowsAsync(new ArgumentException("Weapon not found"));

            // act
            var result = await _controller.Create(dto);

            // assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Weapon not found");
        }

        // ---------- UPDATE ----------

        [Fact]
        public async Task Update_Returns_Ok_When_Skin_Updated()
        {
            // arrange
            var id = 5;
            var dto = new UpdateSkinDto
            {
                WeaponId = 1,
                Name = "Updated Skin",
                PatternStyle = "fade_gun"
            };

            var updated = new SkinDto(id, "Updated Skin", "fade_gun");

            _serviceMock
                .Setup(s => s.UpdateSkinAsync(id, dto))
                .ReturnsAsync(updated);

            // act
            var result = await _controller.Update(id, dto);

            // assert
            var ok = result.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.Value.Should().BeEquivalentTo(updated);
        }

        [Fact]
        public async Task Update_Returns_NotFound_When_Service_Returns_Null()
        {
            // arrange
            var id = 42;
            var dto = new UpdateSkinDto
            {
                WeaponId = 1,
                Name = "Missing Skin",
                PatternStyle = "float_gun"
            };

            _serviceMock
                .Setup(s => s.UpdateSkinAsync(id, dto))
                .ReturnsAsync((SkinDto?)null);

            // act
            var result = await _controller.Update(id, dto);

            // assert
            var notFound = result.Result as NotFoundResult;
            notFound.Should().NotBeNull();
        }

        [Fact]
        public async Task Update_Returns_Conflict_When_Service_Throws_InvalidOperationException()
        {
            // arrange
            var id = 5;
            var dto = new UpdateSkinDto
            {
                WeaponId = 1,
                Name = "Broken",
                PatternStyle = "ch_gun"
            };

            _serviceMock
                .Setup(s => s.UpdateSkinAsync(id, dto))
                .ThrowsAsync(new InvalidOperationException("Cannot change PatternStyle"));

            // act
            var result = await _controller.Update(id, dto);

            // assert
            var conflict = result.Result as ConflictObjectResult;
            conflict.Should().NotBeNull();
            conflict!.Value.Should().Be("Cannot change PatternStyle");
        }

        [Fact]
        public async Task Update_Returns_BadRequest_When_Service_Throws_ArgumentException()
        {
            // arrange
            var id = 5;
            var dto = new UpdateSkinDto
            {
                WeaponId = 999,
                Name = "Invalid",
                PatternStyle = "ch_gun"
            };

            _serviceMock
                .Setup(s => s.UpdateSkinAsync(id, dto))
                .ThrowsAsync(new ArgumentException("Weapon not found"));

            // act
            var result = await _controller.Update(id, dto);

            // assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Weapon not found");
        }

        // ---------- DELETE ----------

        [Fact]
        public async Task Delete_Returns_NoContent_When_Service_Returns_True()
        {
            // arrange
            var id = 7;

            _serviceMock
                .Setup(s => s.DeleteSkinAsync(id))
                .ReturnsAsync(true);

            // act
            var result = await _controller.Delete(id);

            // assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_Returns_NotFound_When_Service_Returns_False()
        {
            // arrange
            var id = 7;

            _serviceMock
                .Setup(s => s.DeleteSkinAsync(id))
                .ReturnsAsync(false);

            // act
            var result = await _controller.Delete(id);

            // assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
