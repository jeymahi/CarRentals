using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarRentals.API.Controllers;
using CarRentals.API.Data;
using CarRentals.API.Interface;
using CarRentals.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarRentals.Tests
{
    // fake service to control behavior of the controller
    public class FakeReservationService : IReservationService
    {
        public Func<ReservationRequest, Task<Reservation>> OnReserveAsync { get; set; }
        public Func<Guid, Task<bool>> OnDeleteAsync { get; set; }

        public Task<Reservation> ReserveAsync(ReservationRequest request)
        {
            if (OnReserveAsync != null)
                return OnReserveAsync(request);

            // default success
            return Task.FromResult(new Reservation
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                CarType = request.CarType,
                Start = request.Start,
                End = request.End
            });
        }

        public Task<bool> DeleteReservationAsync(Guid id)
        {
            if (OnDeleteAsync != null)
                return OnDeleteAsync(id);

            return Task.FromResult(true);
        }
    }

    public class ReservationsControllerTests
    {
        private CarRentalContext CreateContext(string name)
        {
            var opts = new DbContextOptionsBuilder<CarRentalContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new CarRentalContext(opts);
        }

        private ReservationRequest MakeRequest()
        {
            return new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "CUST-1",
                CarType = (CarType)1,
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            };
        }

        [Fact]
        public async Task CreateReservation_Should_Return_Ok_When_Service_Succeeds()
        {
            var fake = new FakeReservationService();
            var controller = new ReservationsController(fake);

            var dto = MakeRequest();

            var result = await controller.CreateReservation(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var res = Assert.IsType<Reservation>(ok.Value);
            Assert.Equal("CUST-1", res.CustomerId);
        }

        [Fact]
        public async Task CreateReservation_Should_Return_Unauthorized_When_Service_Throws_Unauthorized()
        {
            var fake = new FakeReservationService
            {
                OnReserveAsync = _ => throw new UnauthorizedAccessException("bad key")
            };

            var controller = new ReservationsController(fake);

            var result = await controller.CreateReservation(MakeRequest());

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("bad key", unauthorized.Value);
        }

        [Fact]
        public async Task CreateReservation_Should_Return_BadRequest_When_Service_Throws_InvalidOperation()
        {
            var fake = new FakeReservationService
            {
                OnReserveAsync = _ => throw new InvalidOperationException("no cars")
            };

            var controller = new ReservationsController(fake);

            var result = await controller.CreateReservation(MakeRequest());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("no cars", bad.Value);
        }

        [Fact]
        public async Task CreateReservation_Should_Return_BadRequest_When_Service_Throws_ArgumentException()
        {
            var fake = new FakeReservationService
            {
                OnReserveAsync = _ => throw new ArgumentException("bad date")
            };

            var controller = new ReservationsController(fake);

            var result = await controller.CreateReservation(MakeRequest());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("bad date", bad.Value);
        }

        [Fact]
        public async Task Delete_Should_Return_NoContent_When_Service_Returns_True()
        {
            var fake = new FakeReservationService
            {
                OnDeleteAsync = _ => Task.FromResult(true)
            };
            var controller = new ReservationsController(fake);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_When_Service_Returns_False()
        {
            var fake = new FakeReservationService
            {
                OnDeleteAsync = _ => Task.FromResult(false)
            };
            var controller = new ReservationsController(fake);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_From_Context()
        {
            var ctx = CreateContext(nameof(GetAll_Should_Return_All_From_Context));
            // seed 2 reservations
            ctx.Reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                CustomerId = "A",
                CarType = (CarType)1,
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            });
            ctx.Reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                CustomerId = "B",
                CarType = (CarType)2,
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(2)
            });
            await ctx.SaveChangesAsync();

            var controller = new ReservationsController(new FakeReservationService());

            var result = await controller.GetAll(ctx);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<Reservation>>(ok.Value);
            Assert.Collection(list,
                _ => { },
                _ => { });  // 2 items
        }
    }
}
