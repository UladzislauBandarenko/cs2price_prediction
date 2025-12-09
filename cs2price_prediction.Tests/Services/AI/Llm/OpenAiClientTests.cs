using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using cs2price_prediction.Config;
using cs2price_prediction.Services.AI.Llm;
using FluentAssertions;
using Xunit;

namespace cs2price_prediction.Tests.Services.AI.Llm
{
    public class OpenAiClientTests
    {
        // Простой фейковый HttpMessageHandler, чтобы перехватывать запросы и возвращать кастомный ответ
        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public HttpRequestMessage? LastRequest { get; private set; }

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                LastRequest = request;
                var response = _handler(request);
                return Task.FromResult(response);
            }
        }

        private static OpenAiOptions CreateOptions(
            string? apiKey = "test-key",
            string? baseUrl = "https://custom-host/api",
            string? primaryModel = "primary-model")
        {
            return new OpenAiOptions
            {
                ApiKey = apiKey ?? string.Empty,
                BaseUrl = baseUrl,
                PrimaryModel = primaryModel
            };
        }

        private static (OpenAiClient client, FakeHttpMessageHandler handler) CreateClient(
            Func<HttpRequestMessage, HttpResponseMessage> handlerFunc,
            OpenAiOptions? options = null)
        {
            var handler = new FakeHttpMessageHandler(handlerFunc);
            var httpClient = new HttpClient(handler);

            var opts = options ?? CreateOptions();

            var client = new OpenAiClient(httpClient, opts);
            return (client, handler);
        }

        [Fact]
        public void Ctor_Throws_When_ApiKey_Is_Missing()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler);
            var options = CreateOptions(apiKey: null);

            // Act
            Action act = () => _ = new OpenAiClient(httpClient, options);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("OpenAI API key is not configured.");
        }

        [Fact]
        public async Task QueryAsync_Uses_ModelOverride_And_Builds_Correct_Url_And_Body()
        {
            // Arrange
            var options = CreateOptions(
                apiKey: "abc",
                baseUrl: "https://my-endpoint/v1",
                primaryModel: "primary-from-config");

            HttpRequestMessage? capturedRequest = null;
            string jsonResponse = @"{ ""output_text"": ""ok"" }";

            var (client, handler) = CreateClient(req =>
            {
                capturedRequest = req;
                var msg = new HttpResponseMessage(HttpStatusCode.OK);
                msg.Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");
                return msg;
            }, options);

            var prompt = "test prompt";

            // Act
            var result = await client.QueryAsync(prompt, modelOverride: "override-model");

            // Assert результат
            result.Should().Be("ok");

            capturedRequest.Should().NotBeNull();
            handler.LastRequest.Should().BeSameAs(capturedRequest);

            // Проверяем URL
            capturedRequest!.RequestUri!.ToString()
                .Should().Be("https://my-endpoint/v1/responses");

            // Проверяем тело запроса
            var body = await capturedRequest.Content!.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            root.GetProperty("model").GetString().Should().Be("override-model");
            root.GetProperty("input").GetString().Should().Be(prompt);
            root.GetProperty("temperature").GetDouble().Should().Be(0.1);
        }

        [Fact]
        public async Task QueryAsync_Parses_OutputText_Field()
        {
            // Arrange
            var json = @"{ ""output_text"": ""Hello\nworld  test"" }";

            var (client, _) = CreateClient(_ =>
            {
                var msg = new HttpResponseMessage(HttpStatusCode.OK);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return msg;
            });

            // Act
            var result = await client.QueryAsync("prompt");

            // Assert: убраны переносы строк и лишние пробелы
            result.Should().Be("Hello world test");
        }

        [Fact]
        public async Task QueryAsync_Parses_Output_Array_Text_String()
        {
            // Arrange: output[0].content[0].text как строка
            var json = @"
            {
              ""output"": [
                {
                  ""content"": [
                    { ""text"": ""From array text"" }
                  ]
                }
              ]
            }";

            var (client, _) = CreateClient(_ =>
            {
                var msg = new HttpResponseMessage(HttpStatusCode.OK);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return msg;
            });

            // Act
            var result = await client.QueryAsync("prompt");

            // Assert
            result.Should().Be("From array text");
        }

        [Fact]
        public async Task QueryAsync_Parses_Output_Array_Text_Object_Value()
        {
            // Arrange: output[0].content[0].text.value
            var json = @"
            {
              ""output"": [
                {
                  ""content"": [
                    { ""text"": { ""value"": ""Value inside object"" } }
                  ]
                }
              ]
            }";

            var (client, _) = CreateClient(_ =>
            {
                var msg = new HttpResponseMessage(HttpStatusCode.OK);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return msg;
            });

            // Act
            var result = await client.QueryAsync("prompt");

            // Assert
            result.Should().Be("Value inside object");
        }

        [Fact]
        public async Task QueryAsync_Returns_FailedParse_Message_When_No_Text_Found()
        {
            // Arrange: нет ни output_text, ни output[].content[].text
            var json = @"{ ""some_other_field"": ""xxx"" }";

            var (client, _) = CreateClient(_ =>
            {
                var msg = new HttpResponseMessage(HttpStatusCode.OK);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return msg;
            });

            // Act
            var result = await client.QueryAsync("prompt");

            // Assert
            result.Should().StartWith("Failed to parse LLM response. Raw JSON:");
            result.Should().Contain("some_other_field");
        }

        [Fact]
        public async Task QueryAsync_Throws_HttpRequestException_On_NonSuccess_Status()
        {
            // Arrange
            var (client, _) = CreateClient(_ =>
            {
                var msg = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "Server error",
                    Content = new StringContent("Error body")
                };
                return msg;
            });

            // Act
            Func<Task> act = async () => await client.QueryAsync("prompt");

            // Assert
            var ex = await act.Should().ThrowAsync<HttpRequestException>();
            ex.Which.Message.Should().Contain("OpenAI error 500");
            ex.Which.Message.Should().Contain("Error body");
        }

        [Fact]
        public async Task QueryAsync_Cleans_Newlines_Tabs_And_DoubleSpaces()
        {
            // Arrange
            var json = @"{ ""output_text"": ""Line1\r\nLine2\t  Line3"" }";

            var (client, _) = CreateClient(_ =>
            {
                var msg = new HttpResponseMessage(HttpStatusCode.OK);
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return msg;
            });

            // Act
            var result = await client.QueryAsync("prompt");

            // Assert
            result.Should().Be("Line1 Line2 Line3");
            result.Should().NotContain("\r");
            result.Should().NotContain("\n");
            result.Should().NotContain("\t");
        }
    }
}
