using System.Collections.Generic;
using System.Threading.Tasks;
using cs2price_prediction.Controllers;
using cs2price_prediction.DTOs.Prediction;
using cs2price_prediction.Services.Prediction;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Controllers
{
    public class PredictionControllerTests
    {
        private readonly Mock<IPredictionService> _serviceMock;
        private readonly PredictionController _controller;

        public PredictionControllerTests()
        {
            _serviceMock = new Mock<IPredictionService>();
            _controller = new PredictionController(_serviceMock.Object);
        }

        private static PredictionRequestDto CreateSampleDto()
        {
            return new PredictionRequestDto
            {
                SkinId = 1,
                WearTierId = 2,
                FloatValue = 0.1234,
                IsStattrak = true,
                Pattern = 777,
                Stickers = new List<int> { 10, 20, 30 }
            };
        }

        [Fact]
        public async Task Predict_Forwards_Call_To_Service_And_Returns_Same_Result()
        {
            // arrange
            var dto = CreateSampleDto();
            var serviceResult = new OkObjectResult(new { price = 123.45 });

            _serviceMock
                .Setup(s => s.PredictAsync(dto))
                .ReturnsAsync(serviceResult);

            // act
            var result = await _controller.Predict(dto);

            // assert
            result.Should().BeSameAs(serviceResult);
            _serviceMock.Verify(s => s.PredictAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Predict_Also_Works_With_NonOkResult()
        {
            // например BadRequest из сервиса
            var dto = CreateSampleDto();
            var serviceResult = new BadRequestObjectResult("Invalid input");

            _serviceMock
                .Setup(s => s.PredictAsync(dto))
                .ReturnsAsync(serviceResult);

            var result = await _controller.Predict(dto);

            result.Should().BeSameAs(serviceResult);
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
