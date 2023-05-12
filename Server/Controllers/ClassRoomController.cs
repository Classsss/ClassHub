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
        public ClassRoom GetClassRoom(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);
            var query = 
                "SELECT * " +
                "FROM classroom " +
                "WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var result = connection.QuerySingle<ClassRoom>(query, parameters); // ID는 고유하므로, 하나만 반환되는 것이 자명하여 QuerySingle 사용
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
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var result = connection.Query<LectureMaterial>(query, parameters);
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
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var result = connection.Query<Notice>(query, parameters);
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

            InsertNotification(new ClassRoomNotification {
                room_id = lectureMaterial.room_id,
                message = $"(수정) {lectureMaterial.title}",
                uri = $"classroom/{lectureMaterial.room_id}/notice/{lectureMaterial.material_id}",
                notify_date = DateTime.Now
            });
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

            InsertNotification(new ClassRoomNotification {
                room_id = lectureMaterial.room_id,
                message = lectureMaterial.title,
                uri = $"classroom/{lectureMaterial.room_id}/notice/{lectureMaterial.material_id}",
                notify_date = DateTime.Now
            });
        }

        // 수정 된 Notice 객체를 DB에 UPDATE 합니다.
        // 실제 요청 url 예시 : 'api/classroom/modify/notice'
        [HttpPut("modify/notice")]
        public void PutNotice([FromBody] Notice notice) {
            using(var connection = new NpgsqlConnection(connectionString)) {
                string query =
                "UPDATE notice " +
                "SET (title, contents, up_date) = (@title, @contents, @up_date) " +
                "WHERE room_id = @room_id AND notice_id = @notice_id;";
                connection.Execute(query, notice);
            }

            InsertNotification(new ClassRoomNotification {
                room_id = notice.room_id,
                message = $"(수정) {notice.title}",
                uri = $"classroom/{notice.room_id}/notice/{notice.notice_id}",
                notify_date = DateTime.Now
            });
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
            using(var connection = new NpgsqlConnection(connectionString)) {
                string query =
                    "INSERT INTO notice (room_id, title, author, contents, publish_date, up_date, view_count) " +
                    "VALUES (@room_id, @title, @author, @contents, @publish_date, @up_date, @view_count);";
                connection.Execute(query, notice);
            }

            InsertNotification(new ClassRoomNotification {
                room_id = notice.room_id,
                message = notice.title,
                uri = $"classroom/{notice.room_id}/notice/{notice.notice_id}",
                notify_date = DateTime.Now
            });
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

        // Param으로 받은 학번을 가진 학생에게 온 모든 강의실 알림을 불러옴 (모든 수강 강의)
        // 실제 요청 url 예시 : 'api/classroom/notification/all/60182147'
        [HttpGet("notification/all/{student_id}")]
        public IEnumerable<ClassRoomNotification> GetAllNotifications(int student_id) {
            _logger.LogInformation($"GetAllNotifications?student_id={student_id}");
            using var connection = new NpgsqlConnection(connectionString);
            var query =
                "SELECT * " +
                "FROM studentnotification " +
                "WHERE student_id = @student_id;";
            var parameters = new DynamicParameters();
            parameters.Add("student_id", student_id);
            var studentNotifications = connection.Query<StudentNotification>(query, parameters);

            List<ClassRoomNotification> result = new List<ClassRoomNotification>();
            foreach(var item in studentNotifications) {
                query =
                    "SELECT * " +
                    "FROM classroomnotification " +
                    "WHERE notification_id = @notification_id;";
                parameters = new DynamicParameters();
                parameters.Add("notification_id", item.notification_id);
                result.Add(connection.QuerySingle<ClassRoomNotification>(query, parameters));
            }
            return result;
        }

        // 강의실 테이블에 알림을 등록합니다.
        // 아직은 클라이언트에서 직접적으로 알림 INSERT를 요청하는 작업이 없기에 API를 생성하지 않습니다.
        public void InsertNotification(ClassRoomNotification roomNotification) {
            _logger.LogInformation($"InsertNotification?room_id={roomNotification.room_id}; notification_id={roomNotification.notification_id})");
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open(); // 트랜잭션을 사용하는 경우 직접 Open() 하는 것을 권장

            // 동시성 문제 때문에 여러 개의 쿼리를 마치 하나의 작업처럼 실행시키도록 하기 위해 트랜잭션 단위로 묶습니다.
            using(var transaction = connection.BeginTransaction()) {
                try {
                    string query1 = // 다음 notification_id 시퀀스를 가져옴. (반환값은 최초일 경우 1이 나오지만, 쿼리가 실행된 직후 실제 DB내 시퀀스는 2로 바뀜)
                        "SELECT nextval('classroomnotification_notification_id_seq');";
                    roomNotification.notification_id = connection.QuerySingle<int>(sql: query1, transaction: transaction);

                    string query2 = // ClassRoomNotification 테이블에 데이터를 INSERT
                        "INSERT INTO classroomnotification (room_id, notification_id, message, uri, notify_date) " +
                        "VALUES (@room_id, @notification_id, @message, @uri, @notify_date);";
                    var query2_params = new DynamicParameters();
                    query2_params.Add("room_id", roomNotification.room_id);
                    query2_params.Add("notification_id", roomNotification.notification_id);
                    query2_params.Add("message", roomNotification.message);
                    query2_params.Add("uri", roomNotification.uri);
                    query2_params.Add("notify_date", roomNotification.notify_date);
                    connection.Execute(query2, query2_params);

                    string query3 = // 해당 ClassRoom에서 강의를 수강 중인 모든 학생의 학번을 SELECT
                        "SELECT student_id " +
                        "FROM student " +
                        "WHERE room_id = @room_id;";
                    var query3_params = new DynamicParameters();
                    query3_params.Add("room_id", roomNotification.room_id);
                    var students = connection.Query<int>(query3, query3_params);

                    foreach(var student_id in students) {
                        string query4 = // 학번별로 StudentNotification 테이블에 데이터를 INSERT
                            "INSERT INTO studentnotification (room_id, student_id, notification_id, is_read) " +
                            "VALUES (@room_id, @student_id, @notification_id, @is_read);";
                        var query4_params = new DynamicParameters();
                        query4_params.Add("room_id", roomNotification.room_id);
                        query4_params.Add("student_id", student_id);
                        query4_params.Add("notification_id", roomNotification.notification_id);
                        query4_params.Add("is_read", false);
                        connection.Execute(query4, query4_params);
                    }

                    // 모든 쿼리가 성공적으로 실행되면 트랜잭션 커밋
                    transaction.Commit();
                    _logger.LogInformation($"{roomNotification.room_id}번 강의실의 {roomNotification.notification_id}번 알림 INSERT");
                } catch(Exception ex) {
                    _logger.LogError("알림을 INSERT 하던 중 문제가 발생하여 RollBack 합니다.");
                    _logger.LogError($"msg :\n{ex.Message}");
                    transaction.Rollback();
                }
            }
        }
    }
}
