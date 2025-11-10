using System.Threading.Tasks;
using CarRentals.API.Models;

namespace CarRentals.API.Interface
{
    public interface IInventoryService
    {
        Task<CarInventoryEntity> AddOrUpdateAsync(CarInventoryEntity entity);
    }
}
