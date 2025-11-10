using System.Threading.Tasks;
using CarRentals.API.Data;
using CarRentals.API.Interface;
using CarRentals.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarRentals.API.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly CarRentalContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            CarRentalContext context,
            ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CarInventoryEntity> AddOrUpdateAsync(CarInventoryEntity entity)
        {
            // key = CarType
            var existing = await _context.CarInventories
                .FirstOrDefaultAsync(c => c.CarType == entity.CarType);

            if (existing == null)
            {
                _context.CarInventories.Add(entity);
                _logger.LogInformation("Added inventory for CarType {CarType} with capacity {Capacity}",
                    entity.CarType, entity.Capacity);
            }
            else
            {
                existing.Capacity = entity.Capacity;
                _logger.LogInformation("Updated inventory for CarType {CarType} to capacity {Capacity}",
                    entity.CarType, entity.Capacity);
            }

            await _context.SaveChangesAsync();
            return entity;
        }

    }
}
