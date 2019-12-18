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
        public async Task<ActionResult<StoreResponse>> GetStoresByLocation(double lat, double lng, int radius)
        {
            List<StoreResponse> result = await storeService.GetStoresByLocation(lat, lng, radius);
            return Ok(result);
        }
    }
}