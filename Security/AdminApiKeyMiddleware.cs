using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace cs2price_prediction.Security
{
    /// <summary>
    /// Middleware that protects all /api/admin/* endpoints using a static API key.
    /// The key can be provided via appsettings.json or environment variables.
    /// </summary>
    public class AdminApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminApiKeyMiddleware> _logger;

        public AdminApiKeyMiddleware(RequestDelegate next, ILogger<AdminApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration config)
        {
            var path = context.Request.Path;

            // Apply API key validation only for /api/admin/* routes
            if (path.StartsWithSegments("/api/admin"))
            {
                // 1) Read the configured admin API key from all possible sources
                var configuredKey =
                    config["AdminAuth:ApiKey"] ??
                    config["AdminAuth__ApiKey"] ??
                    Environment.GetEnvironmentVariable("ADMIN_AUTH_APIKEY");

                // If no key configured → block admin endpoints entirely
                if (string.IsNullOrWhiteSpace(configuredKey))
                {
                    _logger.LogWarning(
                        "AdminAuth API key is not configured (AdminAuth:ApiKey / AdminAuth__ApiKey / ADMIN_AUTH_APIKEY are empty). Blocking admin endpoints."
                    );

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Admin API is not configured properly.");
                    return;
                }

                // 2) Check the incoming request header
                if (!context.Request.Headers.TryGetValue("X-Admin-Token", out var providedKey))
                {
                    _logger.LogWarning("Missing X-Admin-Token header for admin path {Path}", path);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: missing admin token.");
                    return;
                }

                // 3) Validate the provided key
                if (!string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Invalid admin API key for admin path {Path}", path);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: invalid admin token.");
                    return;
                }
            }

            // Continue the middleware pipeline
            await _next(context);
        }
    }
}
