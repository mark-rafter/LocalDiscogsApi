using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService storeService;

        public StoreController(IStoreService storeService)
        {
            this.storeService = storeService;
        }

        [HttpGet("by-location")]
        public async Task<ActionResult<UserWantlist>> GetStoresByLocation(double lng, double lat, int radiusKm)
        {
            //
            return Ok();
        }
    }
}