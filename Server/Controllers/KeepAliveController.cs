using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace kTVCSSBlazor.Server.Controllers
{
    [ApiController]
    [Route("api")]
    public class KeepAliveController : ControllerBase
    {
        [HttpPost("ping")]
        public IActionResult Ping()
        {
            return Ok(new { timestamp = DateTime.UtcNow, status = "alive" });
        }
    }
}
