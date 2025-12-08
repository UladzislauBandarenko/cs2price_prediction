using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using cs2price_prediction.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;


namespace cs2price_prediction.Tests.Security
{
    public class AdminApiKeyMiddlewareTests
    {
        private static HttpContext CreateContext(string path)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = path;
            ctx.Response.Body = new MemoryStream();   // чтобы можно было прочитать текст ответа
            return ctx;
        }

        private static string GetResponseBody(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        [Fact]
        public async Task InvokeAsync_Should_Skip_Checks_For_NonAdmin_Path()
        {
            // arrange
            var context = CreateContext("/api/public/anything");

            var loggerMock = new Mock<ILogger<AdminApiKeyMiddleware>>();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            };

            var config = new ConfigurationBuilder().Build();

            var middleware = new AdminApiKeyMiddleware(next, loggerMock.Object);

            // act
            await middleware.InvokeAsync(context, config);

            // assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_500_When_ApiKey_Not_Configured()
        {
            // arrange
            var context = CreateContext("/api/admin/panel");

            // убедимся, что переменная окружения не мешает
            Environment.SetEnvironmentVariable("ADMIN_AUTH_APIKEY", null);

            var config = new ConfigurationBuilder().Build(); // без ключей

            var loggerMock = new Mock<ILogger<AdminApiKeyMiddleware>>();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AdminApiKeyMiddleware(next, loggerMock.Object);

            // act
            await middleware.InvokeAsync(context, config);

            // assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            GetResponseBody(context).Should().Contain("Admin API is not configured properly.");
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_401_When_Header_Missing()
        {
            // arrange
            var context = CreateContext("/api/admin/panel");

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("AdminAuth:ApiKey", "secret-key")
                })
                .Build();

            var loggerMock = new Mock<ILogger<AdminApiKeyMiddleware>>();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AdminApiKeyMiddleware(next, loggerMock.Object);

            // act
            await middleware.InvokeAsync(context, config);

            // assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            GetResponseBody(context).Should().Contain("missing admin token");
        }

        [Fact]
        public async Task InvokeAsync_Should_Return_401_When_Header_Invalid()
        {
            // arrange
            var context = CreateContext("/api/admin/panel");
            context.Request.Headers["X-Admin-Token"] = "wrong";

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("AdminAuth:ApiKey", "secret-key")
                })
                .Build();

            var loggerMock = new Mock<ILogger<AdminApiKeyMiddleware>>();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new AdminApiKeyMiddleware(next, loggerMock.Object);

            // act
            await middleware.InvokeAsync(context, config);

            // assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            GetResponseBody(context).Should().Contain("invalid admin token");
        }

        [Fact]
        public async Task InvokeAsync_Should_Call_Next_When_ApiKey_Valid()
        {
            // arrange
            var context = CreateContext("/api/admin/panel");
            context.Request.Headers["X-Admin-Token"] = "secret-key";

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("AdminAuth:ApiKey", "secret-key")
                })
                .Build();

            var loggerMock = new Mock<ILogger<AdminApiKeyMiddleware>>();

            var nextCalled = false;
            RequestDelegate next = ctx =>
            {
                nextCalled = true;
                ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                return Task.CompletedTask;
            };

            var middleware = new AdminApiKeyMiddleware(next, loggerMock.Object);

            // act
            await middleware.InvokeAsync(context, config);

            // assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
    }
}
