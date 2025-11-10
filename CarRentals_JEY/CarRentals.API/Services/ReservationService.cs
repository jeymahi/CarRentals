// Services/ReservationService.cs
using CarRentals.API.Data;
using CarRentals.API.Interface;
using CarRentals.API.Models;
using CarRentals.API.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarRentals.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly CarRentalContext _context;
        private readonly IMemoryCache _cache;
        private readonly ApiKeyValidator _apiKeyValidator;
        private readonly ILogger<ReservationService> _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static string CapacityKey(int carType) => $"capacity:{carType}";

        public ReservationService(
            CarRentalContext context,
            IMemoryCache cache,
            ApiKeyValidator apiKeyValidator,
            ILogger<ReservationService> logger)
        {
            _context = context;
            _cache = cache;
            _apiKeyValidator = apiKeyValidator;
            _logger = logger;
        }

        public async Task<Reservation> ReserveAsync(ReservationRequest request)
        {
            _logger.LogInformation("Reservation request received for Customer {CustomerId}, CarType {CarType}", 
                request.CustomerId, request.CarType);

            // keep API key validation here
            if (!_apiKeyValidator.IsValid(request.ApiKey))
                throw new UnauthorizedAccessException("Invalid API key!");

            var end = request.End;

            InputValidator.EnsureCustomer(request.CustomerId);
            InputValidator.EnsureDateRange(request.Start, end);

            await _lock.WaitAsync();
            try
            {
                var capacity = await GetCapacityAsync(request.CarType);
                if (capacity <= 0)
                    throw new InvalidOperationException("Capacity not configured!");

                var overlappingCount = await _context.Reservations
                    .Where(r => r.CarType == request.CarType)
                    .Where(r => request.Start < r.End && r.Start < end)
                    .CountAsync();

                if (overlappingCount >= capacity)
                    throw new InvalidOperationException("No cars available for that slot!");

                var reservation = new Reservation
                {
                    CustomerId = request.CustomerId,
                    CarType = request.CarType,
                    Start = request.Start,
                    End = end
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return reservation;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> DeleteReservationAsync(Guid id)
        {
            await _lock.WaitAsync();
            try
            {
                var entity = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (entity == null)
                    return false;

                _context.Reservations.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<int> GetCapacityAsync(CarType carType)
        {
            var key = CapacityKey((int)carType);

            if (_cache.TryGetValue<int>(key, out var cached))
                return cached;

            var capacityEntity = await _context.CarInventories
                .FirstOrDefaultAsync(c => c.CarType == carType);

            if (capacityEntity == null)
            {
                _cache.Set(key, 0, TimeSpan.FromMinutes(1));
                return 0;
            }

            _cache.Set(key, capacityEntity.Capacity, TimeSpan.FromMinutes(5));
            return capacityEntity.Capacity;
        }
    }
}
