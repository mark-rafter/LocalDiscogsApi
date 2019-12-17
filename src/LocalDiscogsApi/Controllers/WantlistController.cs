using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WantlistController : ControllerBase
    {
        private readonly IWantlistService wantlistService;

        public WantlistController(IWantlistService wantlistService)
        {
            this.wantlistService = wantlistService;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<UserWantlist>> Get(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            bool wantlistExists = await wantlistService.Exists(username);

            if (!wantlistExists)
            {
                return NotFound();
            }

            return await wantlistService.Get(username);
        }
    }
}