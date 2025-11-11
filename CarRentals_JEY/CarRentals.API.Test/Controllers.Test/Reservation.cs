using System;
using System.Threading.Tasks;
using CarRentals.API.Controllers;
using CarRentals.API.Data;
using CarRentals.API.Models;
using CarRentals.API.Security;
using CarRentals.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CarRentals.API.Test.Controllers
{
    public class ReservationTests
    {
        private CarRentalContext GetContext(string dbName)
        {
            var opts = new DbContextOptionsBuilder<CarRentalContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new CarRentalContext(opts);
        }

        private ReservationService GetService(CarRentalContext ctx)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var apiKeyValidator = new ApiKeyValidator((new[] { "secret-key-123" }).ToString());
            var logger = LoggerFactory.Create(b => b.AddDebug())
                                      .CreateLogger<ReservationService>();

            // 👇 this matches your current ReservationService constructor
            return new ReservationService(
                ctx,
                cache,
                apiKeyValidator,
                logger);
        }

        [Fact]
        public async Task Post_should_return_bad_request_on_overlap()
        {
            // arrange
            var ctx = GetContext("ctrl_overlap");
            // capacity 1 for carType = 1
            ctx.CarInventories.Add(new CarInventoryEntity
            {
                CarType = (CarType)1,
                Capacity = 1
            });
            await ctx.SaveChangesAsync();

            var svc = GetService(ctx);
            var controller = new ReservationsController(svc);

            var start = DateTime.Today.AddHours(9);

            // first one should succeed
            await controller.CreateReservation(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "a",
                CarType = (CarType)1,
                Start = start,
                End = start.AddHours(4)
            });

            // act – second overlaps, should be BadRequest
            var result2 = await controller.CreateReservation(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "b",
                CarType = (CarType)1,
                Start = start.AddHours(2),     // overlaps
                End = start.AddHours(6)
            });

            // assert
            Assert.IsType<BadRequestObjectResult>(result2);
        }
    }
}
