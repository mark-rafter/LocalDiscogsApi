using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Controllers
{
    [Route("debug")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly IWantlistService wantlistService;

        public DebugController(IWantlistService wantlistService)
        {
            this.wantlistService = wantlistService;
        }

        [HttpGet]
        public async Task<ActionResult<UserWantlist>> GetWantlist()
        {
            string username = "forafter";

            var wantlistExists = await wantlistService.Exists(username);

            if (!wantlistExists)
            {
                return NotFound();
            }

            return await wantlistService.Get(username);
        }

        [HttpGet("{username:length(128)}")]
        public async Task<ActionResult<UserWantlist>> GetWantlist(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            var wantlistExists = await wantlistService.Exists(username);

            if (!wantlistExists)
            {
                return NotFound();
            }

            return await wantlistService.Get(username);
        }
    }
}