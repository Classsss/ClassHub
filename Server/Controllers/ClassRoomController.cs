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

        // 실제 요청 url 예시 : 'api/classroom/1/lecturematerial/all' <- 1번 강의실의 모든 강의자료를 불러옴
        [HttpGet("lecturematerial/all/{room_id}")]
        public IEnumerable<LectureMaterial> GetAllLectureMaterialsInClassRoom(int room_id) {
            Console.WriteLine($"room id : {room_id}");
            using var connection = new NpgsqlConnection(connectionString);
            string query = $"SELECT * FROM lecturematerial WHERE \"room_id\" = {room_id};"; // room_id가 동일한 모든 공지사항을 찾습니다.
            var result = connection.Query<LectureMaterial>(query);
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

        // 수정 된 LectureMaterial 객체를 DB에 UPDATE 합니다.
        // 실제 요청 url 예시 : 'api/classroom/modify/lecturematerial'
        [HttpPut("modify/lecturematerial")]
        public void PutLectureMaterial([FromBody] LectureMaterial lectureMaterial) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = 
                "UPDATE lecturematerial " +
                "SET (week, title, contents, up_date) = (@week, @title, @contents, @up_date) " +
                "WHERE room_id = @room_id AND material_id = @material_id;";
            connection.Execute(query, lectureMaterial);
        }

        // LectureMaterial 객체를 DB에 INSERT 합니다.
        // 실제 요청 url 예시 : 'api/classroom/register/lecturematerial'
        [HttpPost("register/lecturematerial")]
        public void PostLectureMaterial([FromBody] LectureMaterial lectureMaterial) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = 
                "INSERT INTO lecturematerial (room_id, week, title, author, contents, publish_date, up_date, view_count) " +
                "VALUES (@room_id, @week, @title, @author, @contents, @publish_date, @up_date, @view_count)";
            connection.Execute(query, lectureMaterial);
        }

        // 수정 된 Notice 객체를 DB에 UPDATE 합니다.
        // 실제 요청 url 예시 : 'api/classroom/modify/notice'
        [HttpPut("modify/notice")]
        public void PutNotice([FromBody] Notice notice) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = 
                "UPDATE notice " +
                "SET (title, contents, up_date) = (@title, @contents, @up_date) " +
                "WHERE room_id = @room_id AND notice_id = @notice_id;";
            connection.Execute(query, notice);
        }

        // Notice 객체를 DB에 INSERT 합니다.
        // 실제 요청 url 예시 : 'api/classroom/register/notice'
        [HttpPost("register/notice")]
        public void PostNotice([FromBody] Notice notice) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = 
                "INSERT INTO notice (room_id, title, author, contents, publish_date, up_date, view_count) " +
                "VALUES (@room_id, @title, @author, @contents, @publish_date, @up_date, @view_count);";
            connection.Execute(query, notice);
        }
    }
}
