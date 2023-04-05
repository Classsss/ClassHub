using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ClassHub.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseController> _logger;

        public CourseController(ILogger<CourseController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> POST()
        {
            //�� ������ �������� ����� ��û
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://academicinfo.azurewebsites.net/Course");
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }
    }
}
