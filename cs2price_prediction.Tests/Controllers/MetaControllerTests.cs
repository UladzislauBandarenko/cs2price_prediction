using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.Controllers;
using cs2price_prediction.DTOs.Meta;
using cs2price_prediction.Services.Meta;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers.Meta
{
    public class MetaControllerTests
    {
        private readonly Mock<IMetaService> _serviceMock;
        private readonly MetaController _controller;

        public MetaControllerTests()
        {
            _serviceMock = new Mock<IMetaService>();
            _controller = new MetaController(_serviceMock.Object);
        }

        // ========== 1) GET /api/meta/weapon-types ==========
        [Fact]
        public async Task GetWeaponTypes_Returns_List_From_Service()
        {
            var expected = new List<WeaponTypeDto>
            {
                new WeaponTypeDto(1, "rifle", "Rifle"),
                new WeaponTypeDto(2, "pistol", "Pistol")
            };

            _serviceMock
                .Setup(s => s.GetWeaponTypesAsync())
                .ReturnsAsync(expected);

            var result = await _controller.GetWeaponTypes();

            result.Should().BeEquivalentTo(expected);
        }

        // ========== 2) GET /api/meta/weapon-types/{weaponTypeId}/weapons ==========
        [Fact]
        public async Task GetWeaponsForType_Returns_List_From_Service()
        {
            int typeId = 10;

            var expected = new List<WeaponDto>
            {
                new WeaponDto(1, "AK-47"),
                new WeaponDto(2, "M4A1-S")
            };

            _serviceMock
                .Setup(s => s.GetWeaponsForTypeAsync(typeId))
                .ReturnsAsync(expected);

            var result = await _controller.GetWeaponsForType(typeId);

            result.Should().BeEquivalentTo(expected);
        }

        // ========== 3) GET /api/meta/weapons/{weaponId}/skins ==========
        [Fact]
        public async Task GetSkinsForWeapon_Returns_List_From_Service()
        {
            int weaponId = 20;

            var expected = new List<SkinDto>
            {
                new SkinDto(1, "Redline",   "float_gun"),
                new SkinDto(2, "Asiimov",   "float_gun")
            };

            _serviceMock
                .Setup(s => s.GetSkinsForWeaponAsync(weaponId))
                .ReturnsAsync(expected);

            var result = await _controller.GetSkinsForWeapon(weaponId);

            result.Should().BeEquivalentTo(expected);
        }

        // ========== 4) GET /api/meta/skins/{skinId}/wear-tiers ==========
        [Fact]
        public async Task GetWearForSkin_Returns_List_From_Service()
        {
            int skinId = 99;

            var expected = new List<WearTierDto>
            {
                new WearTierDto(1, "Factory New"),
                new WearTierDto(2, "Well-Worn")
            };

            _serviceMock
                .Setup(s => s.GetWearForSkinAsync(skinId))
                .ReturnsAsync(expected);

            var result = await _controller.GetWearForSkin(skinId);

            result.Should().BeEquivalentTo(expected);
        }

        // ========== 5) GET /api/meta/skins/{skinId}/patterns ==========
        [Fact]
        public async Task GetPatternsForSkin_Returns_404_If_Skin_Not_Found()
        {
            int skinId = 123;

            _serviceMock
                .Setup(s => s.GetPatternsForSkinAsync(skinId))
                .ReturnsAsync((false, new List<PatternOptionDto>()));

            var result = await _controller.GetPatternsForSkin(skinId);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetPatternsForSkin_Returns_Ok_With_Data()
        {
            int skinId = 123;

            var patterns = new List<PatternOptionDto>
            {
                new PatternOptionDto(1, "Ruby"),
                new PatternOptionDto(2, "Sapphire")
            };

            _serviceMock
                .Setup(s => s.GetPatternsForSkinAsync(skinId))
                .ReturnsAsync((true, patterns));

            var result = await _controller.GetPatternsForSkin(skinId);

            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(patterns);
        }

        // ========== 7) GET /api/meta/stickers?q=...&limit=50 ==========
        [Fact]
        public async Task GetStickers_Returns_List_From_Service()
        {
            string? q = "kato";
            int limit = 50;

            var expected = new List<StickerDto>
            {
                new StickerDto(1, "Kato 2014"),
                new StickerDto(2, "Kato 2015")
            };

            _serviceMock
                .Setup(s => s.GetStickersAsync(q, limit))
                .ReturnsAsync(expected);

            var result = await _controller.GetStickers(q, limit);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
