using System;
using Microsoft.AspNetCore.Mvc;

namespace LocalDiscogsApi.Controllers
{
    [Route("")]
    [ApiController]
    public class AliveController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok($"LocalDiscogsApi ({Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")})" + Environment.NewLine
                + "Service Status: 200 OK" + Environment.NewLine
                + "Server Time: " + DateTime.Now.ToString("o"));
    }
}