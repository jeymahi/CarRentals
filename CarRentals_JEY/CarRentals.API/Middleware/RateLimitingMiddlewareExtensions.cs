using Microsoft.AspNetCore.Builder;

namespace CarRentals.API.Middleware
{
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseReservationRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
