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
        public async Task<IActionResult> getCourses()
        {
            /*
            ���߿� ��ū�� ����Ⱓ, ��ȿ���� �˻��ϴ� �κ��� �ʿ��ϴٰ� �ǴܵǸ� ��û�� �߰��Ұ�
            var client = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://classhubsso.azurewebsites.net/token");

            var authCode = request.AuthorizationCode;
            tokenRequest.Content = new StringContent(JsonSerializer.Serialize(new AccessTokenRequest { AuthorizationCode = authCode }), Encoding.UTF8, "application/json");
                    
            var response = await client.SendAsync(tokenRequest);
            var content = await response.Content.ReadAsStringAsync();
            */
            Console.WriteLine("�л����� ���񽺷� ��û�ϱ�");
            var connectionString = "Host=\r\nacademic-info-db.postgres.database.azure.com\r\n;Username=classhub;Password=ch55361!;Database=AcademicInfo";
            var connection = new NpgsqlConnection(connectionString);

            var query = "SELECT * FROM course";
            var results = connection.Query<CourseResponse>(query);

            connection.Dispose();

            return Ok(results);
        }
    }
}
