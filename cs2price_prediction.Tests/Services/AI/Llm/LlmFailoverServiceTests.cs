using System;
using System.Threading.Tasks;
using cs2price_prediction.Config;
using cs2price_prediction.Services.AI.Llm;
using FluentAssertions;
using Moq;
using Xunit;

namespace cs2price_prediction.Tests.Services.AI.Llm
{
    public class LlmFailoverServiceTests
    {
        private static OpenAiOptions CreateOptions(
            string primary = null,
            string fallback = null)
        {
            return new OpenAiOptions
            {
                ApiKey = "test-key",
                BaseUrl = "https://api.test",
                PrimaryModel = primary,
                FallbackModel = fallback
            };
        }

        [Fact]
        public async Task QueryWithPriorityAsync_MiniThenGpt41_Uses_Primary_First()
        {
            var clientMock = new Mock<ILLMClient>();

            clientMock
                .Setup(c => c.QueryAsync("hello", "mini-model"))
                .ReturnsAsync("ok-mini");

            var options = CreateOptions(primary: "mini-model", fallback: "big-model");
            var service = new LlmFailoverService(clientMock.Object, options);

            var result = await service.QueryWithPriorityAsync("hello", LlmPriority.MiniThenGpt41);

            result.Should().Be("ok-mini");

            clientMock.Verify(c => c.QueryAsync("hello", "mini-model"), Times.Once);
            clientMock.Verify(c => c.QueryAsync("hello", "big-model"), Times.Never);
        }

        [Fact]
        public async Task QueryWithPriorityAsync_MiniThenGpt41_Falls_Back_When_Primary_Fails()
        {
            var clientMock = new Mock<ILLMClient>();

            clientMock
                .Setup(c => c.QueryAsync("hello", "mini-model"))
                .ThrowsAsync(new Exception("boom"));

            clientMock
                .Setup(c => c.QueryAsync("hello", "big-model"))
                .ReturnsAsync("ok-big");

            var options = CreateOptions(primary: "mini-model", fallback: "big-model");
            var service = new LlmFailoverService(clientMock.Object, options);

            var result = await service.QueryWithPriorityAsync("hello", LlmPriority.MiniThenGpt41);

            result.Should().Be("ok-big");

            clientMock.Verify(c => c.QueryAsync("hello", "mini-model"), Times.Once);
            clientMock.Verify(c => c.QueryAsync("hello", "big-model"), Times.Once);
        }

        [Fact]
        public async Task QueryWithPriorityAsync_Gpt41ThenMini_Swaps_Order()
        {
            var clientMock = new Mock<ILLMClient>();

            clientMock
                .Setup(c => c.QueryAsync("hello", "big-model"))
                .ReturnsAsync("ok-big");

            var options = CreateOptions(primary: "mini-model", fallback: "big-model");
            var service = new LlmFailoverService(clientMock.Object, options);

            var result = await service.QueryWithPriorityAsync("hello", LlmPriority.Gpt41ThenMini);

            result.Should().Be("ok-big");

            clientMock.Verify(c => c.QueryAsync("hello", "big-model"), Times.Once);
            clientMock.Verify(c => c.QueryAsync("hello", "mini-model"), Times.Never);
        }

        [Fact]
        public async Task QueryWithPriorityAsync_Uses_Default_Models_When_Not_Configured()
        {
            var clientMock = new Mock<ILLMClient>();

            // Primary пустой → должен использоваться "gpt-4o-mini"
            // Fallback пустой → "gpt-4.1-mini"
            clientMock
                .Setup(c => c.QueryAsync("hello", "gpt-4o-mini"))
                .ReturnsAsync("ok-default");

            var options = CreateOptions(primary: null, fallback: null);
            var service = new LlmFailoverService(clientMock.Object, options);

            var result = await service.QueryWithPriorityAsync("hello", LlmPriority.MiniThenGpt41);

            result.Should().Be("ok-default");

            clientMock.Verify(c => c.QueryAsync("hello", "gpt-4o-mini"), Times.Once);
            clientMock.Verify(c => c.QueryAsync("hello", "gpt-4.1-mini"), Times.Never);
        }

        [Fact]
        public async Task QueryWithPriorityAsync_Default_Priority_Falls_Back_On_Exception()
        {
            var clientMock = new Mock<ILLMClient>();

            // priority = MiniThenGpt41 → primary = primaryConfigured, fallback = fallbackConfigured
            clientMock
                .Setup(c => c.QueryAsync("prompt", "gpt-4o-mini"))
                .ThrowsAsync(new Exception("fail"));

            clientMock
                .Setup(c => c.QueryAsync("prompt", "gpt-4.1-mini"))
                .ReturnsAsync("ok-fallback");

            var options = CreateOptions(primary: null, fallback: null);
            var service = new LlmFailoverService(clientMock.Object, options);

            var result = await service.QueryWithPriorityAsync("prompt", LlmPriority.MiniThenGpt41);

            result.Should().Be("ok-fallback");
        }
    }
}
