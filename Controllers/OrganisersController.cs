
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDayAPI.Database;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDataAccess db = new RaceDayDataAccess();

        private Organiser Map(SqlDataReader r) => new Organiser
        {
            OrganiserId = r.GetString(0),
            FullName = r.GetString(1),
            Email = r.GetString(2),
            PasswordHash = r.GetString(3)
        };

        private const string SelectColumns =
            "ORGANISER_ID, FULL_NAME, EMAIL, PASSWORD_HASH";

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(db.GetAll($"SELECT {SelectColumns} FROM ORGANISER", Map));

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var record = db.GetById(
                $"SELECT {SelectColumns} FROM ORGANISER WHERE ORGANISER_ID = @id",
                id, Map);

            return record == null ? NotFound() : Ok(record);
        }

        [HttpPost]
        public IActionResult Create(Organiser model)
        {
            if (string.IsNullOrWhiteSpace(model.OrganiserId) ||
                string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.PasswordHash))
                return BadRequest("All required organiser fields must be supplied.");

            if (model.OrganiserId.Length != 5)
                return BadRequest("OrganiserId must contain exactly 5 characters.");

            string query = @"INSERT INTO ORGANISER
                (ORGANISER_ID, FULL_NAME, EMAIL, PASSWORD_HASH)
                VALUES (@id, @name, @email, @password)";

            db.Execute(query, c =>
            {
                c.Parameters.AddWithValue("@id", model.OrganiserId);
                c.Parameters.AddWithValue("@name", model.FullName);
                c.Parameters.AddWithValue("@email", model.Email);
                c.Parameters.AddWithValue("@password", model.PasswordHash);
            });

            return CreatedAtAction(nameof(GetById),
                new { id = model.OrganiserId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, Organiser model)
        {
            if (id != model.OrganiserId)
                return BadRequest("The URL ID must match OrganiserId.");

            string query = @"UPDATE ORGANISER SET
                FULL_NAME = @name, EMAIL = @email, PASSWORD_HASH = @password
                WHERE ORGANISER_ID = @id";

            int rows = db.Execute(query, c =>
            {
                c.Parameters.AddWithValue("@id", id);
                c.Parameters.AddWithValue("@name", model.FullName);
                c.Parameters.AddWithValue("@email", model.Email);
                c.Parameters.AddWithValue("@password", model.PasswordHash);
            });

            return rows == 0 ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            int rows = db.Execute(
                "DELETE FROM ORGANISER WHERE ORGANISER_ID = @id",
                c => c.Parameters.AddWithValue("@id", id));

            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
