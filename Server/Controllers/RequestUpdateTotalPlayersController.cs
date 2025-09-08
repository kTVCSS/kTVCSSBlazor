using kTVCSSBlazor.Client.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace kTVCSSBlazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestUpdateTotalPlayersController : ControllerBase
    {
        [HttpGet]
        public async Task Get()
        {
#if DEBUG
            var apiEndpoints = new List<string>()
            {
                "http://localhost:3000",
            };
#endif

#if RELEASE 
            var apiEndpoints = new List<string>()
            {
                "https://api.ktvcss.com",
            };
#endif

            Task.Run(async () =>
            {
                foreach (var endpoint in apiEndpoints)
                {
                    using (var http = new HttpClient())
                    {
                        var response = await http.GetAsync($"{endpoint}/api/RequestUpdateTotalPlayers");
                    }
                }
            });
        }
    }
}
