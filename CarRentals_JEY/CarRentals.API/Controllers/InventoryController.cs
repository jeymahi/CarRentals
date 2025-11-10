// Controllers/InventoryController.cs
using System.Threading.Tasks;
using CarRentals.API.Interface;
using CarRentals.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarRentals.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            IInventoryService inventoryService,
            ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<IActionResult> AddOrUpdate([FromBody] CarInventoryEntity model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _inventoryService.AddOrUpdateAsync(model);
            return Ok(result);
        }
       
    }
}
