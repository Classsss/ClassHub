using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]

    public class LectureController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 학생이 해당 강의 진행률을 가지고 있는지 확인 후 없으면 생성한다.
        [HttpPost("hasprogress/room_id/{room_id}/lecture_id/{lecture_id}/student_id/{student_id}")]
        public void CheckLectureProgress(int room_id,int lecture_id,int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query = "SELECT * FROM lectureprogress WHERE lecture_id = @lecture_id AND student_id = @student_id;";
            var parametersProgress = new DynamicParameters();
            parametersProgress.Add("lecture_id", lecture_id);
            parametersProgress.Add("student_id", student_id);
            var lectureProgress = connection.QuerySingleOrDefault<LectureProgress>(query, parametersProgress);

            if (lectureProgress == null) {
                query = "INSERT INTO lectureprogress (room_id, lecture_id, student_id, elapsed_time, is_enroll) VALUES (@room_id, @lecture_id, @student_id, @elapsed_time, @is_enroll);";
                parametersProgress.Add("room_id",room_id);
                parametersProgress.Add("elapsed_time", 0);
                parametersProgress.Add("is_enroll", false);
                connection.Execute(query, parametersProgress);
            } 
        }

        // 교수가 학생들의 강의 진행률을 확인한다.
        [HttpGet("progress/{lecture_id}")]
        public List<LectureProgress> CheckLectureProgress(int lecture_id) {

            using var connection = new NpgsqlConnection(connectionString);
            string query = "SELECT * FROM lectureprogress WHERE lecture_id = @lecture_id";
            var parametersProgress = new DynamicParameters();
            parametersProgress.Add("lecture_id", lecture_id);
            List<LectureProgress> lectureProgressList = connection.Query<LectureProgress>(query, parametersProgress).ToList();

            return lectureProgressList;
        }

    }
    public class LectureHubController : Hub {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        private static Dictionary<string, bool> isDatabaseWatcherRunning = new Dictionary<string, bool>();

        // 1분마다 강의수강기록을 업데이트한다.
        public async Task UpdateLectureProgressWatcher(int lecture_id,int student_id) {

            string connectionId = Context.ConnectionId;
            Console.WriteLine(connectionId);
            using var connection = new NpgsqlConnection(connectionString);
            isDatabaseWatcherRunning[connectionId] = true;
            while (isDatabaseWatcherRunning[connectionId]) {
                // 테스트를 위해 1초마다 업데이트 원래는 60초마다 해야함
                await Task.Delay(1000);
                Console.WriteLine("test" + student_id);
                string query = "UPDATE lectureprogress SET elapsed_time = elapsed_time +1 WHERE lecture_id = @lecture_id AND student_id = @student_id";
                var parametersUpdate = new DynamicParameters();
                parametersUpdate.Add("lecture_id", lecture_id);
                parametersUpdate.Add("student_id", student_id);
                connection.Execute(query, parametersUpdate);

               // 수강시간을 넘었는지 확인후 수강완료 처리.
                query = "SELECT learning_time From lecture WHERE lecture_id = @lecture_id";
                int learning_time = connection.Query<int>(query,parametersUpdate).FirstOrDefault();

                query = "SELECT elapsed_time From lectureprogress WHERE lecture_id = @lecture_id AND student_id = @student_id";
                int elapsed_time = connection.Query<int>(query, parametersUpdate).FirstOrDefault();

                if(elapsed_time >= learning_time) {
                   query = "UPDATE lectureprogress SET is_enroll = TRUE WHERE lecture_id = @lecture_id AND student_id = @student_id";
                    connection.Execute(query, parametersUpdate);
                }
            }
            isDatabaseWatcherRunning.Remove(connectionId);
        }

        // 연결이 끊길시 호출
        public override async Task OnDisconnectedAsync(Exception exception) {
            string connectionId = Context.ConnectionId;
            await base.OnDisconnectedAsync(exception);
            // 연결 종료 시 isDatabaseWatcher의 while 루프를 빠져나오게끔 함
            if (isDatabaseWatcherRunning.ContainsKey(connectionId)) {
                isDatabaseWatcherRunning[connectionId] = false;
            }
        }
    }
}
