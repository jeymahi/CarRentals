// Interface/IReservationService.cs
using System.Threading.Tasks;
using CarRentals.API.Models;

namespace CarRentals.API.Interface
{
    public interface IReservationService
    {
        Task<Reservation> ReserveAsync(ReservationRequest request);
        Task<bool> DeleteReservationAsync(Guid id);
    }
}
