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
            query = "SELECT * FROM lecture WHERE room_id = @room_id  AND video_url != @video_url ORDER BY week DESC";
            var parametersLecture = new DynamicParameters();
            parametersLecture.Add("room_id", room_id);
            parametersLecture.Add("video_url", "none");
            var sharedLectureList = connection.Query<Shared.Lecture>(query,parametersLecture).ToList();

            int weekCurs = 1;
            int chapterCurs = 1;
            // 동영상 강의에 대하여 진행률을 찾아서 Lecture 모델을 완성시킵니다.
            for (int i = 0; i<sharedLectureList.Count();i++ ) {
                query = "SELECT * FROM lectureprogress WHERE lecture_id = @lecture_id AND student_id = @student_id;";
                var parametersProgress = new DynamicParameters();
                parametersProgress.Add("lecture_id", sharedLectureList[i].lecture_id);
                parametersProgress.Add("student_id", student_id);
                var lectureProgress = connection.QuerySingleOrDefault<LectureProgress>(query, parametersProgress);
                if(lectureProgress !=null) {
                    if (sharedLectureList[i].week == weekCurs) {
                        clientLectureList.Add(new Client.Models.Lecture { RoomId = room_id, LectureId = sharedLectureList[i].lecture_id, Week = sharedLectureList[i].week,Chapter = chapterCurs, Title = sharedLectureList[i].title, Description = sharedLectureList[i].contents, StartDate = sharedLectureList[i].start_date, EndDate = sharedLectureList[i].end_date, VideoUrl = sharedLectureList[i].video_url, RequireLearningTime = sharedLectureList[i].learning_time, CurrentLearningTime = lectureProgress.elapsed_time, IsEnrolled = lectureProgress.is_enroll });
                    } else {
                        clientLectureList.Add(new Client.Models.Lecture { RoomId = room_id, LectureId = sharedLectureList[i].lecture_id, Week = sharedLectureList[i].week, Chapter = 1, Title = sharedLectureList[i].title, Description = sharedLectureList[i].contents, StartDate = sharedLectureList[i].start_date, EndDate = sharedLectureList[i].end_date, VideoUrl = sharedLectureList[i].video_url, RequireLearningTime = sharedLectureList[i].learning_time, CurrentLearningTime = lectureProgress.elapsed_time, IsEnrolled = lectureProgress.is_enroll });
                        weekCurs = sharedLectureList[i].week;
                       chapterCurs = 1;
                    }
                } else {
                    if (sharedLectureList[i].week == weekCurs) {
                        clientLectureList.Add(new Client.Models.Lecture { RoomId = room_id, LectureId = sharedLectureList[i].lecture_id, Week = sharedLectureList[i].week, Chapter = chapterCurs, Title = sharedLectureList[i].title, Description = sharedLectureList[i].contents, StartDate = sharedLectureList[i].start_date, EndDate = sharedLectureList[i].end_date, VideoUrl = sharedLectureList[i].video_url, RequireLearningTime = sharedLectureList[i].learning_time, CurrentLearningTime = 0, IsEnrolled = false });
                    } else {
                        clientLectureList.Add(new Client.Models.Lecture { RoomId = room_id, LectureId = sharedLectureList[i].lecture_id, Week = sharedLectureList[i].week, Chapter = 1, Title = sharedLectureList[i].title, Description = sharedLectureList[i].contents, StartDate = sharedLectureList[i].start_date, EndDate = sharedLectureList[i].end_date, VideoUrl = sharedLectureList[i].video_url, RequireLearningTime = sharedLectureList[i].learning_time, CurrentLearningTime = 0, IsEnrolled = false });
                        weekCurs = sharedLectureList[i].week;
                        chapterCurs = 1;
                    }
                 
                }
                chapterCurs++;
            }
            return clientLectureList;
        }
    }
}
