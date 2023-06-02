using ClassHub.Client.Models;
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

        // db에서 시험 정보를 가져옴
        [HttpGet("room_id/{room_id}/exam_id/{exam_id}/student_id/{student_id}")]
        public async Task<Client.Models.Exam> GetExam(int room_id, int exam_id, int student_id) {
            // db에서 가져오는 Exam
            Shared.Exam dbExam = new Shared.Exam();

            // db에서 해당 시험을 찾는다.
            using var connection = new NpgsqlConnection(connectionString);
            string query = "SELECT * FROM exam WHERE room_id = @room_id AND exam_id = @exam_id;";
            var parametersExam = new DynamicParameters();
            parametersExam.Add("room_id", room_id);
            parametersExam.Add("exam_id", exam_id);
            dbExam = (await connection.QueryAsync<Shared.Exam>(query, parametersExam)).FirstOrDefault();

            // TODO : 시험 제출 내역이 있는지 check
            //query = "SELECT  COUNT(*) FROM assignmentsubmit WHERE room_id = @room_id AND assignment_id = @assignment_id AND student_id = @student_id;";
            //var parametersSubmit = new DynamicParameters();
            //parametersSubmit.Add("room_id", dbExam.room_id);
            //parametersSubmit.Add("assignment_id", dbExam.exam_id);
            //parametersSubmit.Add("student_id", student_id);
            //bool is_submit = true;
            //if (connection.QueryFirstOrDefault<int>(query, parametersSubmit) == 0) { is_submit = false; }

            // Client.Models.Exam에 데이터를 채운다.
            Client.Models.Exam clientExam = new Client.Models.Exam {
                Id = dbExam.exam_id,
                Title = dbExam.title,
                Author = dbExam.author,
                StartDate = dbExam.start_date,
                EndDate = dbExam.end_date,
                IsRandomProblem = dbExam.is_random_problem,
                IsRandomChoice = dbExam.is_random_choice,
                IsShowTimeLimit = dbExam.is_show_time_limit,
                IsBackToPreviousProblem = dbExam.is_back_to_previous_problem,
            };

            // 시험에 등록된 문제들을 찾아 Client.Models.Exam의 Problems 리스트에 넣는다.
            // 객관식 문제
            query = "SELECT * FROM MultipleChoiceProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            var dbMultipleChoiceProblems = await connection.QueryAsync<Shared.MultipleChoiceProblem>(query, parametersExam);

            foreach (var dbMultipleChoiceProblem in dbMultipleChoiceProblems) {
                Client.Models.MultipleChoiceProblem problem = new Client.Models.MultipleChoiceProblem {
                    ProblemId = dbMultipleChoiceProblem.problem_id,
                    Description = dbMultipleChoiceProblem.description,
                    Score = dbMultipleChoiceProblem.score,
                    Answer = dbMultipleChoiceProblem.answer
                };

                // 현재 객관식 문제의 MultipleChoice를 찾는다.
                query = "SELECT * FROM MultipleChoice WHERE problem_id = @problem_id AND exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id)";
                var parametersMultipleChoice = new DynamicParameters();
                parametersMultipleChoice.Add("problem_id", problem.ProblemId);
                parametersMultipleChoice.Add("exam_id", exam_id);
                parametersMultipleChoice.Add("room_id", room_id);
                var dbMultipleChoices = await connection.QueryAsync<Shared.MultipleChoice>(query, parametersMultipleChoice);

                foreach (var dbMultipleChoice in dbMultipleChoices) {
                    problem.Questions.Add(dbMultipleChoice.description);
                }

                clientExam.Problems.Add(problem);
            }

            // 단답형 문제
            query = "SELECT * FROM ShortAnswerProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            var dbShortAnswerProblems = await connection.QueryAsync<Shared.ShortAnswerProblem>(query, parametersExam);

            foreach (var dbShortAnswerProblem in dbShortAnswerProblems) {
                clientExam.Problems.Add(new Client.Models.ShortAnswerProblem { 
                    ProblemId = dbShortAnswerProblem.problem_id,
                    Description = dbShortAnswerProblem.description,
                    Score = dbShortAnswerProblem.score,
                    Answer = dbShortAnswerProblem.answer
                });
            }

            // 코드형 문제
            query = "SELECT * FROM CodingExamProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            var dbCodingExamProblems = await connection.QueryAsync<Shared.CodingExamProblem>(query, parametersExam);

            foreach (var dbCodingExamProblem in dbCodingExamProblems) {
                clientExam.Problems.Add(new Client.Models.CodingProblem {
                    ProblemId = dbCodingExamProblem.problem_id,
                    Description = dbCodingExamProblem.description,
                    Score = dbCodingExamProblem.score,
                    Example = dbCodingExamProblem.example_code,
                    Answer = dbCodingExamProblem.answer_code,
                });
            }

            // Problems 리스트를 ProblemId를 기준으로 오름차순 정렬하고 반환한다.
            clientExam.Problems.Sort((Client.Models.ExamProblem x, Client.Models.ExamProblem y) => x.ProblemId.CompareTo(y.ProblemId));
            return clientExam;
        }

        // 시험을 등록함
        [HttpPost("register")]
        public async Task<IActionResult> RegisterExam([FromBody] Shared.ExamContainer examContainer) {
            if (examContainer == null || examContainer.Exam == null) {
                return BadRequest();
            }

            Shared.Exam exam = examContainer.Exam;
            List<Shared.MultipleChoiceProblemContainer> multipleChoiceProblemContainers = examContainer.MultipleChoiceProblemContainers;
            List<Shared.ShortAnswerProblem> shortAnswerProblems = examContainer.ShortAnswerProblems;
            List<Shared.CodingExamProblem> codingExamProblems = examContainer.CodingExamProblems;

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using (var transaction = connection.BeginTransaction()) {
                try {
                    string query = // 다음 exam_id 시퀀스를 가져옴.
                        "SELECT nextval('exam_exam_id_seq');";
                    exam.exam_id = await connection.QuerySingleAsync<int>(sql: query, transaction: transaction);

                    query = "INSERT INTO Exam (exam_id, room_id, week, title, author, start_date, end_date, is_random_problem, is_random_choice, is_show_time_limit, is_back_to_previous_problem) VALUES (@exam_id, @room_id, @week, @title, @author, @start_date, @end_date, @is_random_problem, @is_random_choice , @is_show_time_limit, @is_back_to_previous_problem);";
                    await connection.ExecuteAsync(query, exam);

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

        // 시험 삭제
        [HttpDelete("{room_id}/remove/{exam_id}")]
        public async Task RemoveExam(int room_id, int exam_id) {
            using var connection = new NpgsqlConnection(connectionString);

            string query = string.Empty;
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            parameters.Add("exam_id", exam_id);

            // 객관식 문제들의 problem_id들을 찾는다.
            query = "SELECT problem_id FROM MultipleChoiceProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            var problem_ids = await connection.QueryAsync<int>(query, parameters);

            // 객관식 보기들 삭제
            foreach (var problem_id in problem_ids) {
                query = "DELETE FROM MultipleChoice WHERE problem_id = @problem_id AND exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id)";
                var parametersMultipleChoice = new DynamicParameters();
                parametersMultipleChoice.Add("problem_id", problem_id);
                parametersMultipleChoice.Add("exam_id", exam_id);
                parametersMultipleChoice.Add("room_id", room_id);
                await connection.ExecuteAsync(query, parametersMultipleChoice);
            }

            // 객관식 문제 삭제
            query = "DELETE FROM MultipleChoiceProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            await connection.ExecuteAsync(query, parameters);

            // 단답형 문제 삭제
            query = "DELETE FROM ShortAnswerProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            await connection.ExecuteAsync(query, parameters);

            // 코드형 문제 삭제
            query = "DELETE FROM CodingExamProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            await connection.ExecuteAsync(query, parameters);

            // 시험 데이터 삭제
            query = "DELETE FROM Exam WHERE room_id = @room_id AND exam_id = @exam_id";
            await connection.ExecuteAsync(query, parameters);
        }
    }
}
