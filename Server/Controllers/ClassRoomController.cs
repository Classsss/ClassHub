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

		private readonly ILogger<ClassRoomController> _logger;

		public ClassRoomController(ILogger<ClassRoomController> logger) {
			_logger = logger;
		}

		// Param으로 받은 ID를 가진 강의실의 정보를 불러옴
		// 실제 요청 url 예시 : 'api/classroom/1'
		[HttpGet("{room_id}")]
        public ClassRoom? GetClassRoom(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);
            var query = 
                "SELECT * " +
                "FROM classroom " +
                "WHERE room_id = @room_id;";
            var result = connection.Query<ClassRoom>(query, room_id).FirstOrDefault(); // ID는 고유하므로, 하나만 반환되는 것이 자명하여 FirstOrDefault()를 통해 첫 번째 요소를 반환합니다. (없으면 기본값)
            return result;
        }

        // Param으로 받은 ID를 가진 강의실의 모든 강의자료를 불러옴
        // 실제 요청 url 예시 : 'api/classroom/1/lecturematerial/all'
        [HttpGet("{room_id}/lecturematerial/all")]
        public IEnumerable<LectureMaterial> GetLectureMaterialListInClassRoom(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = 
                "SELECT * " +
                "FROM lecturematerial " +
                "WHERE room_id = @room_id;";
            var result = connection.Query<LectureMaterial>(query, room_id);
            return result;
        }

        // Param으로 받은 ID를 가진 강의실의 모든 공지사항을 불러옴
        // 실제 요청 url 예시 : 'api/classroom/1/notice/all'
        [HttpGet("{room_id}/notice/all")]
        public IEnumerable<Notice> GetNoticeListInClassRoom(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query =
                "SELECT * " +
                "FROM notice " +
                "WHERE room_id = @room_id;";
            var result = connection.Query<Notice>(query, room_id);
            return result;
        }

        // Param으로 받은 학번을 가진 학생에게 온 모든 알림을 불러옴 (모든 수강 강의)
        // 실제 요청 url 예시 : 'api/classroom/notification/all/60182147'
        [HttpGet("notification/all/{student_id}")]
        public IEnumerable<StudentNotification> GetAllNotifications(int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            var query = 
                "SELECT * " +
                "FROM studentnotification " +
                "WHERE student_id = @student_id;";
            var result = connection.Query<StudentNotification>(query, student_id);
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
                "VALUES (@room_id, @week, @title, @author, @contents, @publish_date, @up_date, @view_count);";
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

        // Board 조회수를 1 증가시켜 DB에 UPDATE 합니다.
        // 실제 요청 url 예시 : 'api/classroom/1/view/notice/1'
        [HttpPut("{room_id}/view/{kind}/{content_id}")]
        public void IncreaseViewCount(int room_id, string kind, int content_id) {
            if(kind != "notice" && kind != "material") {
                _logger.LogError("예상치 못한 kind 값");
                return;
            }
			using var connection = new NpgsqlConnection(connectionString);
            string query =
				$"UPDATE {((kind == "material") ? "lecturematerial" : kind)} " +
				$"SET view_count = view_count + 1 " +
				$"WHERE room_id = @room_id AND {kind}_id = @content_id;";
			var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            parameters.Add("content_id", content_id);
            connection.Execute(query, parameters);
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

		// 공지사항을 삭제합니다
		// 실제 요청 url 예시 : 'api/classroom/1/delete/notice/1'
		[HttpDelete("{room_id}/delete/notice/{notice_id}")]
		public void DeleteNotice(int room_id, int notice_id) {
			using var connection = new NpgsqlConnection(connectionString);
			string query =
				"DELETE FROM notice " +
				"WHERE room_id = @room_id AND notice_id = @notice_id;";
			var parameters = new DynamicParameters();
			parameters.Add("room_id", room_id);
			parameters.Add("notice_id", notice_id);
			connection.Execute(query, parameters);
		}

		// 강의자료를 삭제합니다
		// 실제 요청 url 예시 : 'api/classroom/1/delete/lecturematerial/1'
		[HttpDelete("{room_id}/delete/lecturematerial/{material_id}")]
		public void DeleteLectureMaterial(int room_id, int material_id) {
			using var connection = new NpgsqlConnection(connectionString);
			string query =
				"DELETE FROM lecturematerial " +
				"WHERE room_id = @room_id AND material_id = @material_id;";
			var parameters = new DynamicParameters();
			parameters.Add("room_id", room_id);
			parameters.Add("material_id", material_id);
			connection.Execute(query, parameters);
		}
	}
}
