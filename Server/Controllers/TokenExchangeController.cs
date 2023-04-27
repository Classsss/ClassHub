using ClassHub.Shared;
using Microsoft.AspNetCore.Mvc;
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
    }
}
