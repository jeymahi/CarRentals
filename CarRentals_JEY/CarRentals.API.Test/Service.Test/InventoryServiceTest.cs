using System.Threading.Tasks;
using CarRentals.API.Data;
using CarRentals.API.Models;
using CarRentals.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CarRentals.Tests
{
    public class InventoryServiceTests
    {
        private CarRentalContext CreateContext(string name)
        {
            var opts = new DbContextOptionsBuilder<CarRentalContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new CarRentalContext(opts);
        }

        private ILogger<InventoryService> CreateLogger()
        {
            return LoggerFactory.Create(b => b.AddDebug())
                .CreateLogger<InventoryService>();
        }

        [Fact]
        public async Task AddOrUpdate_Should_Add_When_Not_Exists()
        {
            var ctx = CreateContext(nameof(AddOrUpdate_Should_Add_When_Not_Exists));
            var service = new InventoryService(ctx, CreateLogger());

            var entity = new CarInventoryEntity
            {
                CarType = (CarType)1,
                Capacity = 5
            };

            var saved = await service.AddOrUpdateAsync(entity);

            Assert.Equal((CarType)1, saved.CarType);
            Assert.Equal(5, saved.Capacity);
            Assert.Equal(1, await ctx.CarInventories.CountAsync());
        }

        [Fact]
        public async Task AddOrUpdate_Should_Update_When_Exists()
        {
            var ctx = CreateContext(nameof(AddOrUpdate_Should_Update_When_Exists));
            ctx.CarInventories.Add(new CarInventoryEntity { CarType = (CarType)2, Capacity = 3 });
            await ctx.SaveChangesAsync();

            var service = new InventoryService(ctx, CreateLogger());

            var updated = await service.AddOrUpdateAsync(new CarInventoryEntity
            {
                CarType = (CarType)2,
                Capacity = 10
            });

            Assert.Equal(10, updated.Capacity);
            var fromDb = await ctx.CarInventories.FirstAsync(c => c.CarType == (CarType)2);
            Assert.Equal(10, fromDb.Capacity);
        }
        
    }
}
