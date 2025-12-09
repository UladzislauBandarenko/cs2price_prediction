using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.Controllers;
using cs2price_prediction.DTOs.AI;
using cs2price_prediction.Services.AI.AiExplanation;
using cs2price_prediction.Services.AI.Llm;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers
{
    public class AiExplanationV2ControllerTests
    {
        private readonly Mock<IAiExplanationService> _serviceMock;
        private readonly AiExplanationV2Controller _controller;

        public AiExplanationV2ControllerTests()
        {
            _serviceMock = new Mock<IAiExplanationService>();
            _controller = new AiExplanationV2Controller(_serviceMock.Object);
        }

        [Fact]
        public async Task ExplainV2_Delegates_To_Service_With_Gpt41ThenMini()
        {
            // arrange
            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 5,
                WearTierId = 3,
                FloatValue = 0.456,
                IsStattrak = false,
                Pattern = 101,
                PredictedPrice = 199.99,
                Stickers = new List<int> { 11, 22, 33, 44 } // ✔ корректный List<int>
            };

            var expected = new OkObjectResult(new { explanation = "test explanation" });

            _serviceMock
                .Setup(s => s.ExplainAsync(It.IsAny<AiExplainFrontendInputDto>(), It.IsAny<LlmPriority>()))
                .ReturnsAsync(expected);

            // act
            var result = await _controller.ExplainV2(dto);

            // assert
            result.Should().BeSameAs(expected);

            _serviceMock.Verify(s => s.ExplainAsync(
                It.Is<AiExplainFrontendInputDto>(x =>
                    x.SkinId == dto.SkinId &&
                    x.WearTierId == dto.WearTierId &&
                    x.FloatValue == dto.FloatValue &&
                    x.IsStattrak == dto.IsStattrak &&
                    x.Pattern == dto.Pattern &&
                    x.PredictedPrice == dto.PredictedPrice &&
                    x.Stickers != null &&
                    x.Stickers.Count == dto.Stickers.Count &&
                    x.Stickers[0] == 11 &&
                    x.Stickers[1] == 22 &&
                    x.Stickers[2] == 33 &&
                    x.Stickers[3] == 44
                ),
                LlmPriority.Gpt41ThenMini
            ), Times.Once);
        }
    }
}
