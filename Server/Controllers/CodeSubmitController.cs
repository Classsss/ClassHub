using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Npgsql;

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CodeSubmitController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 초기의 실습 db를 insert한다.
        [HttpPost("insert")]
        public int InsertSubmit([FromBody] CodeSubmit codeSubmit)
        {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            Shared.CodeAssignment codeAssignment = new CodeAssignment(); // 실습 데이터

            // 강의실 번호가 id이며 실습 번호가 practice_id인 실습을 찾습니다.
            query = $"SELECT * FROM codeassignment WHERE \"room_id\" = {codeSubmit.room_id} AND \"assignment_id\" = {codeSubmit.assignment_id};";
            codeAssignment = connection.Query<CodeAssignment>(query).FirstOrDefault();

            // 빈 데이터 채우기
            codeSubmit.week = codeAssignment.week;
            query = "INSERT INTO codesubmit (room_id, week, assignment_id, submit_date, status, student_id, student_name, exec_time,mem_usage,code,message,language) " +
            "VALUES (@room_id, @week, @assignment_id, @submit_date, @status, @student_id,@student_name, @exec_time,@mem_usage,@code,@message,@language);"
            + "SELECT lastval();";

            //DB에서 생성된 submitId를 받아온다.
            int submitId = connection.ExecuteScalar<int>(query, codeSubmit);
            return submitId;
        }

        // 학생이 해당 강의실의 실습번호, 학생 번호에 해당하는 제출리스트들을 불러온다.
        [HttpGet("student/room_id/{room_id}/practice_id/{practice_id}/student_id/{student_id}")]
        public List<CodeSubmit> GetSubmit(int room_id, int practice_id, int student_id){
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            query = "SELECT * FROM codesubmit WHERE room_id = @room_id AND assignment_id = @practice_id AND student_id = @student_id;";

            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            parameters.Add("practice_id", practice_id);
            parameters.Add("student_id", student_id);
            List<CodeSubmit> submitList = connection.Query<CodeSubmit>(query,parameters).ToList();

            return submitList;
        }

        // 교수가 해당 강의실의 실습번호에 해당하는 제출리스트들을 불러온다.
        [HttpGet("professor/room_id/{room_id}/practice_id/{practice_id}")]
        public List<CodeSubmit> GetSubmit(int room_id, int practice_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            query = "SELECT * FROM codesubmit WHERE room_id = @room_id AND assignment_id = @practice_id;";

            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            parameters.Add("practice_id", practice_id);
            List<CodeSubmit> submitList = connection.Query<CodeSubmit>(query, parameters).ToList();

            return submitList;
        }

        // 채점 결과를 업데이트한다.
        [HttpPut("update")]
        public void UpdateSubmit([FromBody] Tuple<JudgeResult, int> result){
            var judgeData = result.Item1;
            var submit_id= result.Item2;

            using var connection = new NpgsqlConnection(connectionString);

            string query;
            Console.WriteLine(judgeData.ExecutionTime);
            query = "UPDATE codesubmit SET  status = @status,  exec_time = @exec_time, mem_usage = @mem_usage WHERE submit_id = @submit_id";
            var parameters = new DynamicParameters();
            parameters.Add("status", judgeData.Result.ToString());
            parameters.Add("exec_time", judgeData.ExecutionTime);
            parameters.Add("mem_usage",judgeData.MemoryUsage);
            parameters.Add("submit_id", submit_id);
            connection.Execute(query,parameters);
        }

        // 채점 결과를 실패로 처리한다.
        [HttpPut("fail")]
        public void JudgeFail([FromBody]int submit_id) {    
            using var connection = new NpgsqlConnection(connectionString);

            string query;

            query = "UPDATE codesubmit SET status = @status WHERE submit_id = @submit_id";
            var parameters = new DynamicParameters();
            parameters.Add("status", JudgeResult.JResult.JudgementFailed.ToString());
            parameters.Add("submit_id", submit_id);
           
            connection.Execute(query, parameters);
        }

    }

    //코드 제출 내역을 위한 Hub
    public class RealTimeSubmitHubController : Hub{
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        private static Dictionary<string, bool> isDatabaseWatcherRunning = new Dictionary<string, bool>();

        // 1초마다 내역에 들어온 클라이언트에게 최신 내역을 제공한다.
        public async Task StartDatabaseWatcher(string query){
            string connectionId = Context.ConnectionId;
            using var connection = new NpgsqlConnection(connectionString);
            isDatabaseWatcherRunning[connectionId] = true;
            while (isDatabaseWatcherRunning[connectionId]) {
                    List<CodeSubmit> submitList = connection.Query<CodeSubmit>(query).ToList();

                    await Clients.Client(connectionId).SendAsync("ReceiveList", submitList);
                    await Task.Delay(1000);
                }
            isDatabaseWatcherRunning.Remove(connectionId);
 
        }
        // 채점서버로부터 진행 현황을 제공받아 제출한 코드의 status를 업데이트한다.
        public async Task PercentageWatcher(double percent, int submit_id){
            string connectionId = Context.ConnectionId;
            using var connection = new NpgsqlConnection(connectionString);
            string query = "UPDATE codesubmit SET status = '채점중(' || @percent || ')%' WHERE submit_id = @submit_id";
            var parameters = new DynamicParameters();
            parameters.Add("percent", percent);
            parameters.Add("submit_id", submit_id);
            connection.Execute(query, parameters);

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
