using BlazorMonaco;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ClassHub.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class SectionController : ControllerBase {
        private readonly ILogger<SectionController> _logger;

        public SectionController(ILogger<SectionController> logger) {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GET(int userId, string accessToken)
        {
            //앱 서버로 수강과목 출력을 요청
            if (await AuthService.isValidToken(userId, accessToken)) {
                Console.WriteLine("성공");
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://academicinfo.azurewebsites.net/");
                //client.BaseAddress = new Uri("https://localhost:7239/");
                var request = new HttpRequestMessage(HttpMethod.Post, "Section");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                return Ok(content);
            } else {
                Console.WriteLine("실패");
                return Unauthorized("Invalid token");
            }
        }
    }
}
