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

        [HttpGet("{id}")]
        public async Task<ActionResult<List<Notice>>> Get(int id)
        {
            var connectionString = $"Host={host};Username={username};Password={passwd};Database={database}";
            using var connection = new NpgsqlConnection(connectionString);

            var query = $"SELECT * FROM Notice WHERE room_id = {id}";
            var notices = await connection.QueryAsync<Notice>(query);
            return notices.ToList();
        }

    }
}
