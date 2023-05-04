using ClassHub.Shared;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ClassHub.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController : ControllerBase
    {
        const string host = "classdb.postgres.database.azure.com";
        const string username = "byungmeo";
        const string passwd = "Mju12345!#";
        const string database = "classdb";

        [HttpGet("{room_id}")]
        public IEnumerable<Notice> Get(int room_id)
        {
            var connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";
            using var connection = new NpgsqlConnection(connectionString);

            var query = $"SELECT * FROM Notice WHERE room_id = {room_id}";
            var notices = connection.Query<Notice>(query);
            return notices;
        }

    }
}
