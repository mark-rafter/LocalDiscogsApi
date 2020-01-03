using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService inventoryService;
        private readonly IWantlistService wantlistService;

        public InventoryController(IInventoryService inventoryService, IWantlistService wantlistService)
        {
            this.inventoryService = inventoryService;
            this.wantlistService = wantlistService;
        }

        [HttpGet("{sellername}")]
        public async Task<ActionResult<SellerInventory>> Get(string sellername)
        {
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

        [HttpGet("in-wantlist")]
        public async Task<ActionResult<FilteredInventoryResponse>> GetInWantlist(string sellername, string wantlistUsername)
        {
            bool wantlistExists = await wantlistService.Exists(wantlistUsername);

            if (!wantlistExists)
            {
                return NotFound($"Discogs wantlist for user: {wantlistUsername} does not exist");
            }

            // todo: re-write Get() to call a version of Exists() that returns the wantlist so we don't call DB twice.
            UserWantlist wantlist = await wantlistService.Get(wantlistUsername);

            if (wantlist?.ReleaseIds?.Any() != true)
            {
                return NotFound($"User: {wantlistUsername} has an empty wantlist");
            }

            SellerInventory sellerInventory = await inventoryService.Get(sellername);

            if (sellerInventory == null)
            {
                return NotFound($"Seller: {sellername} has no listings");
            }
            else
            {
                var result = new FilteredInventoryResponse(sellerInventory, wantlist.ReleaseIds);
                return result;
            }
        }
    }
}