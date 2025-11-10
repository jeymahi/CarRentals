using CarRentals.API.Controllers;
using CarRentals.API.Models;
using CarRentals.API.RateLimiting;
using CarRentals.API.Security;
using CarRentals.API.Services;
using CarRentals.API.Test.Test_Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.API.Test.Controllers.Test
{
    public class Reservation
    {
        [Fact]
        public async Task Post_should_return_bad_request_on_overlap()
        {
            var ctx = DbHelper.GetInMemoryContext("ctrl_overlap");
            ctx.CarInventories.Add(new CarInventoryEntity
            {
                CarType = CarType.Sedan,
                Capacity = 1
            });
            ctx.SaveChanges();

            var svc = new ReservationService(
                ctx,
                new ApiKeyValidator("secret-key-123"),
                new RateLimitingMiddleware(10, TimeSpan.FromSeconds(10)));

            var controller = new ReservationsController(svc);

            var start = DateTime.Today.AddHours(9);

            // first ok
            await controller.CreateReservation(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "a",
                CarType = CarType.Sedan,
                Start = start,
                Days = 1
            });

            // second overlaps → should be BadRequest
            var result2 = await controller.CreateReservation(new ReservationRequest
            {
                ApiKey = "secret-key-123",
                CustomerId = "b",
                CarType = CarType.Sedan,
                Start = start.AddHours(2),
                Days = 1
            });

            Assert.IsType<BadRequestObjectResult>(result2);
        }
    }
}
