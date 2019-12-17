using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            this.inventoryService = inventoryService;
        }

        [HttpGet("{sellername}")]
        public async Task<ActionResult<SellerInventory>> Get(string sellername)
        {
            if (string.IsNullOrEmpty(sellername))
            {
                return NotFound();
            }

            SellerInventory inventory = await inventoryService.Get(sellername);

            if (inventory == null)
            {
                return NotFound();
            }
            else
            {
                return inventory;
            }
        }
    }
}