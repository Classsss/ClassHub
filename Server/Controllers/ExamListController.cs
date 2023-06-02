﻿using ClassHub.Client.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
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
        public List<Submission> GetPracticeList(int room_id, int student_id) {
            using var connection = new NpgsqlConnection(connectionString);

            // 시험 리스트
            List<Submission> examList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            string query = "SELECT * FROM exam WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var exams = connection.Query<Shared.Exam>(query, parameters);

            // TODO : ExamSubmit이 정립되면 Exam 객체를 생성할때 해당 학생의 제출여부 수정 필요

            foreach (Shared.Exam exam in exams) {
                examList.Add(new Client.Models.Exam { Id = exam.exam_id, Title = exam.title, Author = exam.author, StartDate = exam.start_date, EndDate = exam.end_date, IsSubmitted = false, TotalSubmitters = 0 });
            }
            return examList;
        }

        // 해당 강의실에 올라온 시험 리스트를 보여준다.(교수용)
        [HttpGet("professor/room_id/{room_id}")]
        public List<Submission> GetExamList(int room_id) {
            using var connection = new NpgsqlConnection(connectionString);

            // 시험 리스트
            List<Submission> examList = new List<Submission>();

            // 강의실 번호가 room_id인 실습들을 찾습니다.
            string query = "SELECT * FROM exam WHERE room_id = @room_id;";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            var exams = connection.Query<Shared.Exam>(query, parameters);

            // TODO : ExamSubmit이 정립되면 제출한 학생이 몇명인지에 대한 정보를 Exam 객체를 생성할 때 포함해야함

            foreach (Shared.Exam exam in exams) {
                examList.Add(new Client.Models.Exam { Id = exam.exam_id, Title = exam.title, Author = exam.author, StartDate = exam.start_date, EndDate = exam.end_date, IsSubmitted = false, TotalSubmitters = 0 });
            }
            return examList;
        }
    }
}
