using BlazorMonaco;
using ClassHub.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace ClassHub.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenExchangeController : ControllerBase
    {
        private readonly ILogger<TokenExchangeController> _logger;

        public TokenExchangeController(ILogger<TokenExchangeController> logger)
        {
            _logger = logger;
        }

        [HttpPost("submit-code")]
        public async Task<IActionResult> ExchangeToken([FromBody] AccessTokenRequest request)
        {
            var client = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://classhubsso.azurewebsites.net/token");

            var id = request.UserId;
            var authCode = request.AuthorizationCode;
            tokenRequest.Content = new StringContent(JsonSerializer.Serialize(new AccessTokenRequest { UserId = id ,AuthorizationCode = authCode }), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        [HttpGet("verify")]
        public async Task<bool> VerifyToken([FromQuery] int id, string accessToken) {
            var client = new HttpClient();
            var url = $"https://classhubsso.azurewebsites.net/api/token/verify?user_id={id}&accessToken={accessToken}";
            var response = await client.GetAsync(url);

            return bool.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
