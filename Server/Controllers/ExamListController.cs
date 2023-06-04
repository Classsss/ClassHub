using ClassHub.Client.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ExamListController : Controller {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";

        // 해당 강의실에 올라온 과제 리스트를 보여준다.(학생용)
        [HttpGet("student/room_id/{room_id}/student_id/{student_id}")]
        public async Task<List<Submission>> GetPracticeList(int room_id, int student_id) {
            using var connection = new NpgsqlConnection(connectionString);

            // 시험 리스트
            List<Submission> examList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            string query = "SELECT * FROM Exam WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var exams = await connection.QueryAsync<Shared.Exam>(query, parameters);

            foreach (Shared.Exam exam in exams) {
                // 해당 시험을 이 학생이 제출했는지 체크
                query = "SELECT COUNT(*) FROM ExamSubmit WHERE room_id = @room_id AND exam_id = @exam_id AND student_id = @student_id;";
                var submitParameters = new DynamicParameters();
                submitParameters.Add("room_id", room_id);
                submitParameters.Add("exam_id", exam.exam_id);
                submitParameters.Add("student_id", student_id);
                int submitCount = await connection.QueryFirstOrDefaultAsync<int>(query, submitParameters);

                bool isSubmitted = submitCount > 0 ? true : false;

                examList.Add(new Client.Models.Exam { 
                    Id = exam.exam_id, 
                    Title = exam.title, 
                    Author = exam.author, 
                    StartDate = exam.start_date, 
                    EndDate = exam.end_date, 
                    IsSubmitted = isSubmitted, 
                    TotalSubmitters = 0 
                });
            }
            return examList;
        }

        // 해당 강의실에 올라온 시험 리스트를 보여준다.(교수용)
        [HttpGet("professor/room_id/{room_id}")]
        public async Task<List<Submission>> GetExamList(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);

            // 시험 리스트
            List<Submission> examList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            string query = "SELECT * FROM exam WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var exams = await connection.QueryAsync<Shared.Exam>(query, parameters);

            foreach (Shared.Exam exam in exams) {
                // 이 시험의 제출인원 계산
                query = "SELECT COUNT(DISTINCT student_id) FROM ExamSubmit WHERE room_id = @room_id AND exam_id = @exam_id;";
                var submitParameters = new DynamicParameters();
                submitParameters.Add("room_id", room_id);
                submitParameters.Add("exam_id", exam.exam_id);
                int totalSubmitCount = await connection.QueryFirstOrDefaultAsync<int>(query, submitParameters);

                examList.Add(new Client.Models.Exam {
                    Id = exam.exam_id, 
                    Title = exam.title, 
                    Author = exam.author, 
                    StartDate = exam.start_date, 
                    EndDate = exam.end_date, 
                    IsSubmitted = true, 
                    TotalSubmitters = totalSubmitCount 
                });
            }
            return examList;
        }
    }
}
