using CarRentals.API.Data;
using CarRentals.API.Models;
using CarRentals.API.RateLimiting;
using CarRentals.API.Security;
using CarRentals.API.Services;
using CarRentals.API.Test.Test_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.API.Test.Service.Test
{
    public class Reservation
    {
        private async Task<CarRentalContext> SeedContextAsync(string dbName)
        {
            var ctx = DbHelper.GetInMemoryContext(dbName);

            if (!ctx.CarInventories.Any())
            {
                ctx.CarInventories.AddRange(
                    new CarInventoryEntity { CarType = CarType.Sedan, Capacity = 2 },
                    new CarInventoryEntity { CarType = CarType.Suv, Capacity = 1 },
                    new CarInventoryEntity { CarType = CarType.Van, Capacity = 1 }
                );
                await ctx.SaveChangesAsync();
            }

            return ctx;
        }

        private ReservationService BuildService(CarRentalContext ctx)
        {
            var api = new ApiKeyValidator("secret-key-123");
            var rate = new RateLimiter(50, TimeSpan.FromMinutes(1));
            return new ReservationService(ctx, api, rate);
        }

        [Fact]
        public async Task Create_reservation_success()
        {
            var ctx = await SeedContextAsync("db_success");
            var svc = BuildService(ctx);

            var dto = new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "cust1",
                CarType = CarType.Sedan,
                Start = DateTime.Today.AddHours(9).AddMinutes(15),
                Days = 1
            };

            var res = await svc.ReserveAsync(dto);

            Assert.NotNull(res);
            Assert.Equal("cust1", res.CustomerId);
            Assert.Equal(CarType.Sedan, res.CarType);
        }

        [Fact]
        public async Task Overlapping_timestamp_should_fail_when_capacity_full()
        {
            var ctx = await SeedContextAsync("db_overlap");
            var svc = BuildService(ctx);

            var start = DateTime.Today.AddHours(9).AddMinutes(15); // 09:15

            // capacity for sedan = 2 → we will take 2 first
            await svc.ReserveAsync(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "c1",
                CarType = CarType.Sedan,
                Start = start,
                Days = 1
            });

            await svc.ReserveAsync(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "c2",
                CarType = CarType.Sedan,
                Start = start.AddMinutes(10), // still inside window
                Days = 1
            });

            // 3rd overlapping should fail
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.ReserveAsync(new ReservationRequest
                {
                    ApiKey = "secret-key-123",
                    CustomerId = "c3",
                    CarType = CarType.Sedan,
                    Start = start.AddMinutes(30),
                    Days = 1
                });
            });
        }

        [Fact]
        public async Task Exact_end_time_should_be_allowed()
        {
            var ctx = await SeedContextAsync("db_exact_end");
            var svc = BuildService(ctx);

            var start = DateTime.Today.AddHours(9);

            // first booking 1 day
            await svc.ReserveAsync(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "c1",
                CarType = CarType.Sedan,
                Start = start,
                Days = 1
            });

            // second booking starts exactly when the fist ends
            var res2 = await svc.ReserveAsync(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "c2",
                CarType = CarType.Sedan,
                Start = start.AddDays(1), 
                Days = 1
            });

            Assert.NotNull(res2);
        }

        [Fact]
        public async Task Invalid_api_key_should_be_rejected()
        {
            var ctx = await SeedContextAsync("db_invalid_key");
            var svc = BuildService(ctx);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await svc.ReserveAsync(new ReservationRequest
                {
                    ApiKey = "wrong-key",
                    CustomerId = "cust1",
                    CarType = CarType.Sedan,
                    Start = DateTime.Today.AddHours(9),
                    Days = 1
                });
            });
        }

        [Fact]
        public async Task Rate_limit_should_block_after_threshold()
        {
            var ctx = await SeedContextAsync("db_rate");
            var api = new ApiKeyValidator("secret-key-123");
            var rate = new RateLimiter(2, TimeSpan.FromSeconds(30)); // only 2 calls allowed
            var svc = new ReservationService(ctx, api, rate);

            var dto = new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "same-user",
                CarType = CarType.Sedan,
                Start = DateTime.Today.AddHours(9),
                Days = 1
            };

            // Act - first 2 reservations must succeed
            await svc.ReserveAsync(dto);

            await svc.ReserveAsync(new ReservationRequest
            {
                ApiKey = dto.ApiKey,
                CustomerId = dto.CustomerId,
                CarType = dto.CarType,
                Start = dto.Start.AddMinutes(5),
                Days = dto.Days
            });

            // Assert - 3rd reservation should fail due to rate limiter
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await svc.ReserveAsync(new ReservationRequest
                {
                    ApiKey = dto.ApiKey,
                    CustomerId = dto.CustomerId,
                    CarType = dto.CarType,
                    Start = dto.Start.AddMinutes(10),
                    Days = dto.Days
                });
            });
        }
    }
}
