using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;


namespace ClassHub.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]

    public class StudentController : ControllerBase {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";
        const string connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";


        // 해당 강의실의 학생들의 정보를 불러온다.
        [HttpGet("{room_id}")]
        public List<Student> GetStudents(int room_id) {

            using var connection = new NpgsqlConnection(connectionString);
            string query = "SELECT * FROM student WHERE room_id = @room_id";
            var parameters = new DynamicParameters();
            parameters.Add("room_id", room_id);
            List<Student> students = connection.Query<Student>(query, parameters).ToList();

            return students;
        }
    }
}
