using ClassHub.Client.Models;
using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 해당 강의실의 해당 Id와 실습 Id에 맞는 실습을 보여준다.
        [HttpGet("room_id/{room_id}/practice_id/{practice_id}")]
        public Practice GetPractice(int room_id, int practice_id){
                using var connection = new NpgsqlConnection(connectionString);
                string query;

                Shared.CodeAssignment codeAssignment = new CodeAssignment(); // 실습 데이터
                Shared.CodeProblem codeProblem = new Shared.CodeProblem();   // 문제 데이터

                List<string> examInput = new List<string>();                //예제 입력 리스트
                List<string> examoutput = new List<string>();               //예제 출력 리스트
                List<string> input = new List<string>();                    //실제 입력 리스트

                // 강의실 번호가 id이며 실습 번호가 practice_id인 실습을 찾습니다.
                query = "SELECT * FROM codeassignment WHERE room_id = @room_id AND assignment_id = @practice_id;";
                var parametersPractice = new DynamicParameters();
                parametersPractice.Add("room_id", room_id);
                parametersPractice.Add("practice_id", practice_id);
                codeAssignment = connection.Query<CodeAssignment>(query,parametersPractice).FirstOrDefault();

                // 문제 번호가 problem_id인 문제를 찾습니다.      
                query = "SELECT * FROM codeproblem WHERE problem_id = @problem_id;";
                var parametersProblem = new DynamicParameters();
                parametersProblem.Add("problem_id", codeAssignment.problem_id);
                codeProblem = connection.Query<Shared.CodeProblem>(query,parametersProblem).FirstOrDefault();

                // 문제 번호가 problem_id인 예제 테스트케이스를 찾습니다
                query = "SELECT * FROM exampletestcase WHERE problem_id = @problem_id;";
                var exampleTestcases = connection.Query<Shared.ExampleTestcase>(query,parametersProblem);
                foreach (Shared.ExampleTestcase exampleTestcase in exampleTestcases){
                    examInput.Add(exampleTestcase.input);
                    examoutput.Add(exampleTestcase.output);
                }

                // 문제 번호가 problem_id인 실제 테스트케이스를 찾습니다
                query = "SELECT * FROM testcase WHERE problem_id = @problem_id;";
                var testcases = connection.Query<Shared.ExampleTestcase>(query, parametersProblem);
                foreach (Shared.ExampleTestcase testcase in testcases){
                    input.Add(testcase.input);
                }

                //이제 practice모델에 필요한 값들을 넣어줍니다.
                Practice practice = new Practice { Id = codeAssignment.assignment_id, Title = codeProblem.title, Author = codeProblem.author, Content = codeProblem.contents, ExamInputCases = examInput,
                    ExamOutputCases = examoutput, Language = codeProblem.standard_language, isApplyScore = codeAssignment.is_apply_score, StartDate = codeAssignment.start_date, EndDate = codeAssignment.end_date,
                    IntputCases = input, CorrectCode = codeProblem.standard_code };
          
                return practice;      
        }
    }
}
