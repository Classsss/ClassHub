using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeListController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 해당 강의실에 올라온 실습 리스트를 보여준다.(학생용)
        [HttpGet("student/room_id/{room_id}/student_id/{student_id}")]
        public List<Submission> GetPracticeList(int room_id,int student_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            //실습 리스트
            List<Submission> practiceList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            query = "SELECT * FROM codeassignment WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var codeAssignments = connection.Query<CodeAssignment>(query,parameters);

            // 문제 번호가 problem_id인 문제를 찾은 후 practice를 생성하고 practiceList에 추가합니다.
            foreach (CodeAssignment codeAssignment in codeAssignments) {
                query = "SELECT * FROM codeproblem WHERE problem_id = @problem_id;";
                var parametersProblem = new DynamicParameters();
                parametersProblem.Add("problem_id", codeAssignment.problem_id);
                Shared.CodeProblem codeProblem = connection.Query<Shared.CodeProblem>(query, parametersProblem).FirstOrDefault();

                query = "SELECT COUNT(*) FROM codesubmit WHERE room_id = @room_id AND assignment_id = @assignment_id AND student_id = @student_id;";
                var parametersSubmit = new DynamicParameters();
                parametersSubmit.Add("room_id", codeAssignment.room_id);
                parametersSubmit.Add("assignment_id", codeAssignment.assignment_id);
                parametersSubmit.Add("student_id", student_id);
                bool is_submit = true;
                if (connection.QueryFirstOrDefault<int>(query, parametersSubmit) == 0) { is_submit = false; }

                practiceList.Add(new Client.Models.Assignment { Id = codeAssignment.assignment_id, Title = codeProblem.title, Description = codeProblem.contents, Author = codeProblem.author, StartDate = codeAssignment.start_date, EndDate = codeAssignment.end_date, IsSubmitted = is_submit, TotalSubmitters=1 });
            }
            return practiceList;
        }

        // 해당 강의실에 올라온 실습 리스트를 보여준다.(교수용)
        [HttpGet("professor/room_id/{room_id}")]
        public List<Submission> GetPracticeList(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);
            string query;

            //실습 리스트
            List<Submission> practiceList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            query = "SELECT * FROM codeassignment WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var codeAssignments = connection.Query<CodeAssignment>(query, parameters);

            // 문제 번호가 problem_id인 문제를 찾은 후 practice를 생성하고 practiceList에 추가합니다.
            foreach (CodeAssignment codeAssignment in codeAssignments) {
                query = "SELECT * FROM codeproblem WHERE problem_id = @problem_id;";
                var parametersProblem = new DynamicParameters();
                parametersProblem.Add("problem_id", codeAssignment.problem_id);
                Shared.CodeProblem codeProblem = connection.Query<Shared.CodeProblem>(query, parametersProblem).FirstOrDefault();

                query = "SELECT COUNT(DISTINCT student_id) FROM codesubmit WHERE room_id = @room_id AND assignment_id = @assignment_id;";
                var parametersSubmit = new DynamicParameters();
                parametersSubmit.Add("room_id", codeAssignment.room_id);
                parametersSubmit.Add("assignment_id", codeAssignment.assignment_id);
                int count = connection.QueryFirstOrDefault<int>(query, parametersSubmit);
                practiceList.Add(new Client.Models.Assignment { Id = codeAssignment.assignment_id, Title = codeProblem.title, Description = codeProblem.contents, Author = codeProblem.author, StartDate = codeAssignment.start_date, EndDate = codeAssignment.end_date, IsSubmitted = true, TotalSubmitters=count});
            }
            return practiceList;
        }
    }
}
