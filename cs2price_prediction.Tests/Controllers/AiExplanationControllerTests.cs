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
    public class AiExplanationControllerTests
    {
        private readonly Mock<IAiExplanationService> _serviceMock;
        private readonly AiExplanationController _controller;

        public AiExplanationControllerTests()
        {
            _serviceMock = new Mock<IAiExplanationService>();
            _controller = new AiExplanationController(_serviceMock.Object);
        }

        [Fact]
        public async Task Explain_Delegates_To_Service_With_MiniThenGpt41()
        {
            // arrange
            var dto = new AiExplainFrontendInputDto
            {
                SkinId = 1,
                WearTierId = 2,
                FloatValue = 0.1234,
                IsStattrak = true,
                Pattern = 777,
                PredictedPrice = 123.45,
                Stickers = new List<int> { 10, 20, 0, 30 } // ✅ список, не массив
            };

            var expectedResult = new OkObjectResult(new { explanation = "test" });

            _serviceMock
                .Setup(s => s.ExplainAsync(
                    It.IsAny<AiExplainFrontendInputDto>(),
                    It.IsAny<LlmPriority>()))
                .ReturnsAsync(expectedResult);

            // act
            var result = await _controller.Explain(dto);

            // assert
            result.Should().BeSameAs(expectedResult);

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
                        x.Stickers[0] == 10 &&
                        x.Stickers[1] == 20 &&
                        x.Stickers[2] == 0 &&
                        x.Stickers[3] == 30
                    ),
                    LlmPriority.MiniThenGpt41),
                Times.Once);
        }
    }
}
