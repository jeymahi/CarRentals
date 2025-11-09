using CarRentals.API.Models;
using CarRentals.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRentals.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        public ReservationsController(ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationRequest dto)
        {
            try
            {
                var res = await _reservationService.ReserveAsync(dto);
                return Ok(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromServices] Data.CarRentalContext ctx)
        {
            var list = await System.Threading.Tasks.Task
                .FromResult(ctx.Reservations);
            return Ok(list);
        }
    }
}
