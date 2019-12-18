using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        public async Task<ActionResult<List<StoreResponse>>> GetStoresByLocation(double lat, double lng, int radius)
        {
            // todo: async streaming...
            List<StoreResponse> result = await storeService.GetStoresByLocation(lat, lng, radius);
            return result;
        }
    }
}