using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ClassRoomController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";

        [HttpGet("{id}")]
        public ClassRoom Get(int id) {
            var connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";
            var connection = new NpgsqlConnection(connectionString);

            var query = $"SELECT * FROM classroom WHERE \"room_id\" = {id};"; // 강의실 번호가 id인 강의실을 찾습니다.
            var result = connection.Query<ClassRoom>(query).FirstOrDefault(); // ID는 고유하므로, 하나만 반환되는 것이 자명하여 FirstOrDefault()를 통해 첫 번째 요소를 반환합니다. (없으면 기본값)

            connection.Dispose();

            return result;
        }
    }
}
