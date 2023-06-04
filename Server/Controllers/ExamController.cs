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
            dbExam = await connection.QuerySingleAsync<Shared.Exam>(query, parametersExam);

            // 해당 시험을 이 학생이 제출했는지 체크
            query = "SELECT COUNT(*) FROM ExamSubmit WHERE room_id = @room_id AND exam_id = @exam_id AND student_id = @student_id;";
            var submitParameters = new DynamicParameters();
            submitParameters.Add("room_id", room_id);
            submitParameters.Add("exam_id", exam_id);
            submitParameters.Add("student_id", student_id);
            int submitCount = await connection.QueryFirstOrDefaultAsync<int>(query, submitParameters);

            bool isSubmitted = submitCount > 0 ? true : false;

            // 이 시험의 제출인원 계산
            query = "SELECT COUNT(DISTINCT student_id) FROM ExamSubmit WHERE room_id = @room_id AND exam_id = @exam_id;";
            var submitCountParameters = new DynamicParameters();
            submitCountParameters.Add("room_id", room_id);
            submitCountParameters.Add("exam_id", exam_id);
            int totalSubmitCount = await connection.QueryFirstOrDefaultAsync<int>(query, submitCountParameters);

            // Client.Models.Exam에 데이터를 채운다.
            Client.Models.Exam clientExam = new Client.Models.Exam {
                Id = dbExam.exam_id,
                Title = dbExam.title,
                Author = dbExam.author,
                StartDate = dbExam.start_date,
                EndDate = dbExam.end_date,
                IsSubmitted = isSubmitted,
                TotalSubmitters = totalSubmitCount,
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
                query = "SELECT * FROM MultipleChoice WHERE problem_id = @problem_id AND exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                var parametersMultipleChoice = new DynamicParameters();
                parametersMultipleChoice.Add("problem_id", problem.ProblemId);
                parametersMultipleChoice.Add("exam_id", exam_id);
                parametersMultipleChoice.Add("room_id", room_id);
                var dbMultipleChoices = await connection.QueryAsync<Shared.MultipleChoice>(query, parametersMultipleChoice);

                foreach (var dbMultipleChoice in dbMultipleChoices) {
                    problem.Choices.Add(new Client.Models.MultipleChoice { 
                        ChoiceId = dbMultipleChoice.choice_id,
                        Description = dbMultipleChoice.description
                    });
                }
                problem.Choices = problem.Choices.OrderBy(item => item.ChoiceId).ToList();

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
                    // 다음 exam_id 시퀀스를 가져옴.
                    string query = "SELECT nextval('exam_exam_id_seq');";
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

        // 과제 db를 수정합니다
        [HttpPut("modify")]
        public async Task ModifyExam([FromBody] Shared.ExamContainer examContainer) {
            Shared.Exam newExam = examContainer.Exam;
            List<Shared.MultipleChoiceProblemContainer> multipleChoiceProblemContainers = examContainer.MultipleChoiceProblemContainers;
            List<Shared.ShortAnswerProblem> shortAnswerProblems = examContainer.ShortAnswerProblems;
            List<Shared.CodingExamProblem> codingExamProblems = examContainer.CodingExamProblems;

            using var connection = new NpgsqlConnection(connectionString);

            // Exam 업데이트
            string query = "UPDATE Exam SET exam_id = @exam_id, room_id = @room_id, week = @week, title = @title, author = @author, start_date = @start_date, end_date = @end_date, is_random_problem = @is_random_problem, is_random_choice = @is_random_choice, is_show_time_limit = @is_show_time_limit, is_back_to_previous_problem = @is_back_to_previous_problem WHERE room_id = @room_id AND exam_id = @exam_id;";
            var examParameters = new DynamicParameters();
            examParameters.Add("exam_id : " + newExam.exam_id);
            examParameters.Add("room_id", newExam.room_id);
            examParameters.Add("week", newExam.week);
            examParameters.Add("title", newExam.title);
            examParameters.Add("author", newExam.author);
            examParameters.Add("start_date", newExam.start_date);
            examParameters.Add("end_date", newExam.end_date);
            examParameters.Add("is_random_problem", newExam.is_random_problem);
            examParameters.Add("is_random_choice", newExam.is_random_choice);
            examParameters.Add("is_show_time_limit", newExam.is_show_time_limit);
            examParameters.Add("is_back_to_previous_problem", newExam.is_back_to_previous_problem);
            await connection.ExecuteAsync(query, examParameters);

            // 객관식 문제 업데이트
            foreach (var container in multipleChoiceProblemContainers) {
                Shared.MultipleChoiceProblem newProblem = container.MultipleChoiceProblem;

                // 현재 시험 문제가 디비에 존재하는지 확인
                query = "SELECT COUNT(*) FROM MultipleChoiceProblem WHERE exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                var checkParameters = new DynamicParameters();
                checkParameters.Add("exam_id", newExam.exam_id);
                checkParameters.Add("problem_id", newProblem.problem_id);
                checkParameters.Add("room_id", newExam.room_id);
                int examCount = await connection.QueryFirstOrDefaultAsync<int>(query, checkParameters);

                // 이미 디비에 존재함
                if (examCount > 0) {
                    query = "UPDATE MultipleChoiceProblem SET description = @description, answer = @answer, score = @score WHERE exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                    var multipleChoiceProblemparameters = new DynamicParameters();
                    multipleChoiceProblemparameters.Add("description", newProblem.description);
                    multipleChoiceProblemparameters.Add("answer", newProblem.answer);
                    multipleChoiceProblemparameters.Add("score", newProblem.score);
                    multipleChoiceProblemparameters.Add("exam_id", newExam.exam_id);
                    multipleChoiceProblemparameters.Add("problem_id", newProblem.problem_id);
                    multipleChoiceProblemparameters.Add("room_id", newExam.room_id);
                    await connection.ExecuteAsync(query, multipleChoiceProblemparameters);
                }
                // 디비에 존재하지 않는 새로운 문제
                else {
                    string insertProblemQuery = "INSERT INTO MultipleChoiceProblem (exam_id, problem_id, description, answer, score) VALUES (@exam_id, @problem_id, @description, @answer, @score);";
                    await connection.ExecuteAsync(insertProblemQuery, newProblem);
                }

                // 객관식 문제에 대한 보기 업데이트
                List<Shared.MultipleChoice> multipleChoices = container.MultipleChoices;
                foreach (var newChoice in multipleChoices) {
                    // 현재 문제 보기가 디비에 존재하는지 확인
                    query = "SELECT COUNT(*) FROM MultipleChoice WHERE choice_id = @choice_id AND exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                    checkParameters = new DynamicParameters();
                    checkParameters.Add("choice_id", newChoice.choice_id);
                    checkParameters.Add("exam_id", newExam.exam_id);
                    checkParameters.Add("problem_id", newProblem.problem_id);
                    checkParameters.Add("room_id", newExam.room_id);
                    examCount = await connection.QueryFirstOrDefaultAsync<int>(query, checkParameters);

                    // 이미 디비에 존재함
                    if (examCount > 0) {
                        query = "UPDATE MultipleChoice SET description = @description WHERE exam_id = @exam_id AND problem_id = @problem_id AND choice_id = @choice_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                        var multipleChoiceParameters = new DynamicParameters();
                        multipleChoiceParameters.Add("description", newChoice.description);
                        multipleChoiceParameters.Add("exam_id", newExam.exam_id);
                        multipleChoiceParameters.Add("problem_id", newProblem.problem_id);
                        multipleChoiceParameters.Add("choice_id", newChoice.choice_id);
                        multipleChoiceParameters.Add("room_id", newExam.room_id);
                        await connection.ExecuteAsync(query, multipleChoiceParameters);
                    }
                    // 디비에 존재하지 않는 새로운 보기
                    else {
                        string insertChoiceQuery = "INSERT INTO MultipleChoice (exam_id, problem_id, description) VALUES (@exam_id, @problem_id, @description);";
                        await connection.ExecuteAsync(insertChoiceQuery, newChoice);
                    }
                }
            }

            // 단답형 문제 업데이트
            foreach (var newProblem in shortAnswerProblems) {
                // 현재 시험 문제가 디비에 존재하는지 확인
                query = "SELECT COUNT(*) FROM ShortAnswerProblem WHERE exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                var checkParameters = new DynamicParameters();
                checkParameters.Add("exam_id", newExam.exam_id);
                checkParameters.Add("problem_id", newProblem.problem_id);
                checkParameters.Add("room_id", newExam.room_id);
                int examCount = await connection.QueryFirstOrDefaultAsync<int>(query, checkParameters);

                // 이미 디비에 존재함
                if (examCount > 0) {
                    query = "UPDATE ShortAnswerProblem SET description = @description, answer = @answer, score = @score WHERE exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                    var shortAnswerProblemParameters = new DynamicParameters();
                    shortAnswerProblemParameters.Add("description", newProblem.description);
                    shortAnswerProblemParameters.Add("answer", newProblem.answer);
                    shortAnswerProblemParameters.Add("score", newProblem.score);
                    shortAnswerProblemParameters.Add("exam_id", newExam.exam_id);
                    shortAnswerProblemParameters.Add("problem_id", newProblem.problem_id);
                    shortAnswerProblemParameters.Add("room_id", newExam.room_id);
                    await connection.ExecuteAsync(query, shortAnswerProblemParameters);
                }
                // 디비에 존재하지 않는 새로운 문제
                else {
                    string insertProblemQuery = "INSERT INTO ShortAnswerProblem (exam_id, problem_id, description, answer, score) VALUES (@exam_id, @problem_id, @description, @answer, @score);";
                    await connection.ExecuteAsync(insertProblemQuery, newProblem);
                }
            }

            // 코드형 문제 업데이트
            foreach (var newProblem in codingExamProblems) {
                // 현재 시험 문제가 디비에 존재하는지 확인
                query = "SELECT COUNT(*) FROM CodingExamPRoblem WHERE exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                var checkParameters = new DynamicParameters();
                checkParameters.Add("exam_id", newExam.exam_id);
                checkParameters.Add("problem_id", newProblem.problem_id);
                checkParameters.Add("room_id", newExam.room_id);
                int examCount = await connection.QueryFirstOrDefaultAsync<int>(query, checkParameters);

                // 이미 디비에 존재함
                if (examCount > 0) {
                    query = "UPDATE CodingExamProblem SET description = @description, example_code = @example_code, answer_code = @answer_code, score = @score WHERE exam_id = @exam_id AND problem_id = @problem_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
                    var codingExamProblemParameters = new DynamicParameters();
                    codingExamProblemParameters.Add("description", newProblem.description);
                    codingExamProblemParameters.Add("example_code", newProblem.example_code);
                    codingExamProblemParameters.Add("answer_code", newProblem.answer_code);
                    codingExamProblemParameters.Add("score", newProblem.score);
                    codingExamProblemParameters.Add("exam_id", newExam.exam_id);
                    codingExamProblemParameters.Add("problem_id", newProblem.problem_id);
                    codingExamProblemParameters.Add("room_id", newExam.room_id);
                    await connection.ExecuteAsync(query, codingExamProblemParameters);
                }
                // 디비에 존재하지 않는 새로운 문제
                else {
                    string insertProblemQuery = "INSERT INTO CodingExamProblem (exam_id, problem_id, description, example_code, answer_code, score) VALUES (@exam_id, @problem_id, @description, @example_code, @answer_code, @score);";
                    await connection.ExecuteAsync(insertProblemQuery, newProblem);
                }
            }
        }

        // 시험 삭제
        [HttpDelete("{room_id}/remove/{exam_id}")]
        public async Task RemoveExam(int room_id, int exam_id) {
            using var connection = new NpgsqlConnection(connectionString);

            // 객관식 문제 제출 모두 삭제
            string query = "DELETE FROM MultipleChoiceProblemSubmit WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM ExamSubmit WHERE room_id = @room_id);";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            parameters.Add("exam_id", exam_id);
            await connection.ExecuteAsync(query, parameters);

            // 단답형 문제 제출 모두 삭제
            query = "DELETE FROM ShortAnswerProblemSubmit WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM ExamSubmit WHERE room_id = @room_id);";
            await connection.ExecuteAsync(query, parameters);

            // 코드형 문제 제출 모두 삭제
            query = "DELETE FROM CodingExamProblemSubmit WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM ExamSubmit WHERE room_id = @room_id);";
            await connection.ExecuteAsync(query, parameters);

            // 시험 제출 삭제
            query = "DELETE FROM ExamSubmit WHERE exam_id = @exam_id AND room_id = @room_id;";
            await connection.ExecuteAsync(query, parameters);

            // 객관식 문제들의 problem_id들을 찾는다.
            query = "SELECT problem_id FROM MultipleChoiceProblem WHERE exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
            var problem_ids = await connection.QueryAsync<int>(query, parameters);

            // 객관식 보기들 삭제
            foreach (var problem_id in problem_ids) {
                query = "DELETE FROM MultipleChoice WHERE problem_id = @problem_id AND exam_id = @exam_id AND exam_id IN (SELECT exam_id FROM Exam WHERE room_id = @room_id);";
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
            query = "DELETE FROM Exam WHERE room_id = @room_id AND exam_id = @exam_id;";
            await connection.ExecuteAsync(query, parameters);
        }
    }
}
