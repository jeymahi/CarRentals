// Middleware/RateLimitingMiddleware.cs
using CarRentals.API.Middleware;
using CarRentals.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace CarRentals.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RateLimiter rateLimiter)
        {
            // only throttle reservation endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/api/reservation", System.StringComparison.OrdinalIgnoreCase))
            {
                // get client IP
                var ip = context.Connection.RemoteIpAddress;

                // if behind proxy and you want to trust X-Forwarded-For, you could read it here
                // var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                string key = $"reserve-ip:{ip}";

                if (!rateLimiter.Allow(key))
                {
                    _logger.LogWarning("Rate limit exceeded for IP {IP}", ip);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded for this IP");
                    return;
                }
            }

            await _next(context);
        }
    }
}
