using LocalDiscogsApi.Database;
using LocalDiscogsApi.Models;
using LocalDiscogsApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LocalDiscogsApi.Controllers
{
    [Route("debug")]
    [ApiController]
    public class DebugController : Controller
    {
        private readonly IWantlistService wantlistService;

        public DebugController(IWantlistService wantlistService)
        {
            this.wantlistService = wantlistService;
        }

        [HttpGet("wantlist/{username:length(128)}")]
        public async Task<ActionResult<UserWantlist>> GetWantlist(string username)
        {
            var wantlistExists = await wantlistService.Exists(username);

            if (!wantlistExists)
            {
                return NotFound();
            }

            return await wantlistService.Get(username);
        }
    }
}