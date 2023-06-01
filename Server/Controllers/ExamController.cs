using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : Controller { 
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 시험을 등록합니다
        [HttpPost("register")]
        public async Task<IActionResult> RegisterExam([FromBody] Shared.ExamContainer examContainer) {
            if (examContainer == null || examContainer.Exam == null) {
                return BadRequest();
            }

            Shared.Exam exam = examContainer.Exam;
            List<MultipleChoiceProblemContainer> multipleChoiceProblemContainers = examContainer.MultipleChoiceProblemContainers;
            List<Shared.ShortAnswerProblem> shortAnswerProblems = examContainer.ShortAnswerProblems;
            List<Shared.CodingExamProblem> codingExamProblems = examContainer.CodingExamProblems;

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using (var transaction = connection.BeginTransaction()) {
                try {
                    string query1 = // 다음 exam_id 시퀀스를 가져옴.
                        "SELECT nextval('exam_exam_id_seq');";
                    exam.exam_id = await connection.QuerySingleAsync<int>(sql: query1, transaction: transaction);
                    string query2 = "INSERT INTO Exam (exam_id, room_id, week, title, author, start_date, end_date, is_random_problem, is_random_choice, is_show_time_limit, is_back_to_previous_problem) VALUES (@exam_id, @room_id, @week, @title, @author, @start_date, @end_date, @is_random_problem, @is_random_choice , @is_show_time_limit, @is_back_to_previous_problem);";
                    await connection.ExecuteAsync(query2, exam);

                    // 객관식 문제와 보기 INSERT
                    foreach (var entry in multipleChoiceProblemContainers) {
                        // 객관식 문제 INSERT
                        Shared.MultipleChoiceProblem problem = entry.MultipleChoiceProblem;
                        problem.exam_id = exam.exam_id;

                        string insertProblemQuery = "INSERT INTO MultipleChoiceProblem (exam_id, problem_id, description, answer, score) VALUES (@exam_id, @problem_id, @description, @answer, @score);";
                        await connection.ExecuteAsync(insertProblemQuery, problem);

                        // 객관식 문제 보기 INSERT
                        List<Shared.MultipleChoice> multipleChoices = entry.MultipleChoices;
                        foreach (var choice in multipleChoices) {
                            choice.exam_id = exam.exam_id;

                            string insertChoiceQuery = "INSERT INTO MultipleChoice (exam_id, problem_id, description) VALUES (@exam_id, @problem_id, @description);";
                            await connection.ExecuteAsync(insertChoiceQuery, choice);
                        }
                    }

                    // 단답형 문제 INSERT
                    foreach (var problem in shortAnswerProblems) {
                        problem.exam_id = exam.exam_id;

                        string insertProblemQuery = "INSERT INTO ShortAnswerProblem (exam_id, problem_id, description, answer, score) VALUES (@exam_id, @problem_id, @description, @answer, @score);";
                        await connection.ExecuteAsync(insertProblemQuery, problem);
                    }

                    // 코드형 문제 INSERT
                    foreach (var problem in codingExamProblems) {
                        problem.exam_id = exam.exam_id;

                        string insertProblemQuery = "INSERT INTO CodingExamProblem (exam_id, problem_id, description, example_code, answer_code, score) VALUES (@exam_id, @problem_id, @description, @example_code, @answer_code, @score);";
                        await connection.ExecuteAsync(insertProblemQuery, problem);
                    }

                    await transaction.CommitAsync();
                } catch (Exception ex) {
                    Console.WriteLine("Exception catch : " + ex.Message);
                    transaction.Rollback();
                    return BadRequest();
                }
            }

            return Ok(exam.exam_id);
        }
    }
}
