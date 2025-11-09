// Services/ReservationService.cs
using CarRentals.API.Data;
using CarRentals.API.Models;
using CarRentals.API.RateLimiting;
using CarRentals.API.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarRentals.API.Services
{
    public class ReservationService
    {
        private readonly CarRentalContext _ctx;
        private readonly ApiKeyValidator _apiKeyValidator;
        private readonly RateLimiter _rateLimiter;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1); // multi-thread safety

        public ReservationService(
            CarRentalContext ctx,
            ApiKeyValidator apiKeyValidator,
            RateLimiter rateLimiter)
        {
            _ctx = ctx;
            _apiKeyValidator = apiKeyValidator;
            _rateLimiter = rateLimiter;
        }

        public async Task<Reservation> ReserveAsync(ReservationRequest request)
        {
            // Security
            if (!_apiKeyValidator.IsValid(request.ApiKey))
                throw new UnauthorizedAccessException("Invalid API key!");

            // Rate limit per customer
            if (!_rateLimiter.Allow($"reserve:{request.CustomerId}"))
                throw new InvalidOperationException("Rate limit exceeded!");

            var end = request.End;

            // Validate inputs
            InputValidator.EnsureCustomer(request.CustomerId);
            InputValidator.EnsureDateRange(request.Start, end);

            await _lock.WaitAsync();             
            try
            {
                // Get capacity from inmemory DB
                var capacityEntity = await _ctx.CarInventories
                    .FirstOrDefaultAsync(c => c.CarType == request.CarType);

                if (capacityEntity == null)
                    throw new InvalidOperationException("Capacity not configured!");

                var capacity = capacityEntity.Capacity;

                // check overlap in DB, timestamp-accurate
                // overlap: newStart < existing.End AND existing.Start < newEnd
                var overlappingCount = await _ctx.Reservations
                    .Where(r => r.CarType == request.CarType)
                    .Where(r => request.Start < r.End && r.Start < end)
                    .CountAsync();

                if (overlappingCount >= capacity)
                    throw new InvalidOperationException("No cars available for that slot!");

                // create and save
                var reservation = new Reservation
                {
                    CustomerId = request.CustomerId,
                    CarType = request.CarType,
                    Start = request.Start,
                    End = end
                };

                _ctx.Reservations.Add(reservation);
                await _ctx.SaveChangesAsync();

                return reservation;
            }
            finally
            {
                _lock.Release();                  
            }
        }
    }
}
