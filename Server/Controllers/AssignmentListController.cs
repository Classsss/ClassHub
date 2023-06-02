using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentListController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 해당 강의실에 올라온 과제 리스트를 보여준다.(학생용)
        [HttpGet("student/room_id/{room_id}/student_id/{student_id}")]
        public List<Submission> GetPracticeList(int room_id, int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            //과제 리스트
            List<Submission> assignmentList = new List<Submission>();

            // 강의실 번호가 room_id인 과제들을 찾습니다.
            query = "SELECT * FROM assignment WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var Assignments = connection.Query<Shared.Assignment>(query, parameters);

       
            foreach (Shared.Assignment Assignment in Assignments) {
                query = "SELECT  COUNT(*) FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id AND student_id = @student_id;";
                var parametersSubmit = new DynamicParameters();
                parametersSubmit.Add("room_id", Assignment.room_id);
                parametersSubmit.Add("assignment_id", Assignment.assignment_id);
                parametersSubmit.Add("student_id", student_id);
                bool is_submit = true;
                if (connection.QueryFirstOrDefault<int>(query, parametersSubmit) == 0) { is_submit = false; }
                assignmentList.Add(new Client.Models.Assignment { Id = Assignment.assignment_id, Title = Assignment.title, Description = Assignment.contents, Author = Assignment.author, StartDate = Assignment.start_date, EndDate = Assignment.end_date, IsSubmitted = is_submit, TotalSubmitters = 0 });
            }
            return assignmentList;
        }


        // 해당 강의실에 올라온 과제 리스트를 보여준다.(교수용)
        [HttpGet("professor/room_id/{room_id}")]
        public List<Submission> GetPracticeList(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            //실습 리스트
            List<Submission> assignmentList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            query = "SELECT * FROM assignment WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var Assignments = connection.Query<Shared.Assignment>(query, parameters);

            // 과제 번호가 assignment_id인 문제를 찾은 후 assignment를 생성하고 assignment List에 추가합니다.
            foreach (Shared.Assignment Assignment in Assignments) {
                query = "SELECT COUNT(DISTINCT student_id) FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id;";
                var parametersSubmit = new DynamicParameters();
                parametersSubmit.Add("room_id", Assignment.room_id);
                parametersSubmit.Add("assignment_id", Assignment.assignment_id);
                int count = connection.QueryFirstOrDefault<int>(query, parametersSubmit);
                assignmentList.Add(new Client.Models.Assignment { Id = Assignment.assignment_id, Title = Assignment.title, Description = Assignment.contents, Author = Assignment.author, StartDate = Assignment.start_date, EndDate = Assignment.end_date, IsSubmitted = true, TotalSubmitters = count });
            }
            return assignmentList;
        }
    }
}
