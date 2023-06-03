using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data.Common;
using System.Transactions;

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ExamSubmitController : Controller {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 시험을 제출함
        [HttpPost("register")]
        public async Task<IActionResult> RegisterExamSubmit([FromBody] Shared.ExamSubmitContainer examSubmitContainer) {
            if (examSubmitContainer == null || examSubmitContainer.ExamSubmit == null) {
                return BadRequest();
            }

            Shared.ExamSubmit examSubmit = examSubmitContainer.ExamSubmit;
            List<Shared.MultipleChoiceProblemSubmit> multipleChoiceProblemSubmits = examSubmitContainer.MultipleChoiceProblemSubmits;
            List<Shared.ShortAnswerProblemSubmit> shortAnswerProblemSubmits = examSubmitContainer.ShortAnswerProblemSubmits;
            List<Shared.CodingExamProblemSubmit> codingExamProblemSubmits = examSubmitContainer.CodingExamProblemSubmits;

            using var connection = new NpgsqlConnection(connectionString);

            // 다음 submit_id 시퀀스를 가져옴.
            string query = "SELECT nextval('examsubmit_submit_id_seq');";
            examSubmit.submit_id = await connection.QuerySingleAsync<int>(query);

            query = "INSERT INTO examsubmit (submit_id, exam_id, room_id, student_id, student_name, score, submit_date) VALUES (@submit_id, @exam_id, @room_id, @student_id, @student_name, @score, @submit_date);";
            await connection.ExecuteAsync(query, examSubmit);

            // 객관식 문제와 보기 INSERT
            foreach (var submit in multipleChoiceProblemSubmits) {
                submit.submit_id = examSubmit.submit_id;

                string insertSubmitQuery = "INSERT INTO MultipleChoiceProblemSubmit (submit_id, exam_id, problem_id, answer) VALUES (@submit_id, @exam_id, @problem_id, @answer);";
                await connection.ExecuteAsync(insertSubmitQuery, submit);
            }

            // 단답형 문제 INSERT
            foreach (var submit in shortAnswerProblemSubmits) {
                submit.submit_id = examSubmit.submit_id;

                string insertSubmitQuery = "INSERT INTO ShortAnswerProblemSubmit (submit_id, exam_id, problem_id, answer) VALUES (@submit_id, @exam_id, @problem_id, @answer);";
                await connection.ExecuteAsync(insertSubmitQuery, submit);
            }

            // 코드형 문제 INSERT
            foreach (var submit in codingExamProblemSubmits) {
                submit.submit_id = examSubmit.submit_id;

                string insertSubmitQuery = "INSERT INTO CodingExamProblemSubmit (submit_id, exam_id, problem_id, answer_code) VALUES (@submit_id, @exam_id, @problem_id, @answer_code);";
                await connection.ExecuteAsync(insertSubmitQuery, submit);
            }

            return Ok(examSubmit.submit_id);
        }
    }
}
