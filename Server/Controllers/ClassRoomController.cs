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
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 실제 요청 url 예시 : 'api/classroom/1' <- 1번 강의실의 정보를 불러옴
        [HttpGet("{room_id}")]
        public ClassRoom GetClassRoom(int room_id) {
            var connection = new NpgsqlConnection(connectionString);

            var query = $"SELECT * FROM classroom WHERE \"room_id\" = {room_id};"; // 강의실 번호가 id인 강의실을 찾습니다.
            var result = connection.Query<ClassRoom>(query).FirstOrDefault(); // ID는 고유하므로, 하나만 반환되는 것이 자명하여 FirstOrDefault()를 통해 첫 번째 요소를 반환합니다. (없으면 기본값)

            connection.Dispose();

            return result;
        }

        // 실제 요청 url 예시 : 'api/classroom/notification/all/60182147' <- 학번이 60182147인 학생에게 온 모든 알림을 불러옴
        [HttpGet("notification/all/{student_id}")]
        public IEnumerable<StudentNotification> GetAllNotifications(int student_id) {
            var connection = new NpgsqlConnection(connectionString);

            var query = $"SELECT * FROM studentnotification WHERE \"student_id\" = {student_id};"; // student_id가 동일한 모든 알림을 찾습니다.
            var result = connection.Query<StudentNotification>(query);

            connection.Dispose();

            return result;
        }
    }
}
