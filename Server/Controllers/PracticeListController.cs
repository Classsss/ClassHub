using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using System.Linq;

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeListController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";


        // 해당 강의실에 올라온 실습 리스트를 보여준다.
        [HttpGet("{room_id}")]
        public IEnumerable<Assignment> GetPracticeList(int room_id) {
            var connection = new NpgsqlConnection(connectionString);

            string query;


            List<Assignment> practiceList = new List<Assignment>();
            
                query = $"SELECT * FROM codeassignment WHERE \"room_id\" = {room_id};"; // 강의실 번호가 id인 실습들을 찾습니다.
            var codeAssignments = connection.Query<CodeAssignment>(query);

            foreach(CodeAssignment codeAssignment in codeAssignments)
            {
                query = $"SELECT * FROM codeproblem WHERE \"problem_id\" = {codeAssignment.problem_id};"; // 강의실 번호가 id인 강의실을 찾습니다.
                Shared.CodeProblem codeProblem = connection.Query<Shared.CodeProblem>(query).FirstOrDefault();
                practiceList.Add(new Assignment { Id = codeAssignment.assignment_id, Title = codeProblem.title, Description = codeProblem.content, Author = codeProblem.author, StartDate = codeAssignment.start_date, EndDate = codeAssignment.end_date, IsSubmitted = true });
            }
            connection.Dispose();

            IEnumerable<Assignment> practiceEnumerable = practiceList;
            return practiceEnumerable;
        }
    }
}
