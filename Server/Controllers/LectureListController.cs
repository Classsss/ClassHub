using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LectureListController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 해당 강의실에 올라온 동영상강의 리스트를 보여준다.
        [HttpGet("room_id/{room_id}/student_id/{student_id}")]
        public List<Client.Models.Lecture> GetLectureList(int room_id,int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            // 반환할 동영상 리스트
            List<Client.Models.Lecture> clientLectureList = new List<Client.Models.Lecture>();

            // 강의실 번호가 room_id인 동영상강의들을 찾습니다.
            query = "SELECT * FROM lecture WHERE room_id = @room_id  ORDER BY week DESC, chapter ASC;";
            var parametersLecture = new DynamicParameters();
            parametersLecture.Add("room_id", room_id);
            var sharedLectureList = connection.Query<Shared.Lecture>(query,parametersLecture);

            // 동영상 강의에 대하여 진행률을 찾습니다.
            foreach (Shared.Lecture sharedLecture in sharedLectureList) {
                query = "SELECT * FROM lectureprogress WHERE lecture_id = @lecture_id AND student_id = @student_id;";
                var parametersProgress = new DynamicParameters();
                parametersProgress.Add("lecture_id", sharedLecture.lecture_id);
                parametersProgress.Add("student_id", student_id);
                var lectureProgress = connection.QuerySingleOrDefault<LectureProgress>(query, parametersProgress);
                if(lectureProgress !=null) {
                    clientLectureList.Add(new Client.Models.Lecture { RoomId = room_id, LectureId = sharedLecture.lecture_id, Week = sharedLecture.week, Chapter = sharedLecture.chapter, Title = sharedLecture.title, Description = sharedLecture.contents, StartDate = sharedLecture.start_date, EndDate = sharedLecture.end_date, VideoUrl = sharedLecture.video_url, RequireLearningTime = sharedLecture.learning_time, CurrentLearningTime = lectureProgress.elapsed_time, IsEnrolled = lectureProgress.is_enroll });
                } else {
                    clientLectureList.Add(new Client.Models.Lecture { RoomId = room_id, LectureId = sharedLecture.lecture_id, Week = sharedLecture.week, Chapter = sharedLecture.chapter, Title = sharedLecture.title, Description = sharedLecture.contents, StartDate = sharedLecture.start_date, EndDate = sharedLecture.end_date, VideoUrl = sharedLecture.video_url, RequireLearningTime = sharedLecture.learning_time, CurrentLearningTime = 0, IsEnrolled = false });
                }
               
            }
            return clientLectureList;
        }
    }
}
