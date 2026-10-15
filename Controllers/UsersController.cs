
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDayAPI.Database;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDataAccess db = new RaceDayDataAccess();

        private User Map(SqlDataReader r) => new User
        {
            UserId = r.GetString(0),
            FullName = r.GetString(1),
            Email = r.GetString(2),
            PasswordHash = r.GetString(3),
            Role = r.GetString(4)
        };

        private const string SelectColumns =
            "USER_ID, FULL_NAME, EMAIL, PASSWORD_HASH, ROLE";

        [HttpGet]
        public IActionResult GetAll()
        {
            var records = db.GetAll(
                $"SELECT {SelectColumns} FROM USERS", Map);

            return Ok(records);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var record = db.GetById(
                $"SELECT {SelectColumns} FROM USERS WHERE USER_ID = @id",
                id, Map);

            return record == null ? NotFound() : Ok(record);
        }

        [HttpPost]
        public IActionResult Create(User model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) ||
                string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.PasswordHash))
                return BadRequest("All required user fields must be supplied.");

            if (model.UserId.Length != 5)
                return BadRequest("UserId must contain exactly 5 characters.");

            string query = @"INSERT INTO USERS
                (USER_ID, FULL_NAME, EMAIL, PASSWORD_HASH, ROLE)
                VALUES (@id, @name, @email, @password, @role)";

            db.Execute(query, c =>
            {
                c.Parameters.AddWithValue("@id", model.UserId);
                c.Parameters.AddWithValue("@name", model.FullName);
                c.Parameters.AddWithValue("@email", model.Email);
                c.Parameters.AddWithValue("@password", model.PasswordHash);
                c.Parameters.AddWithValue("@role", model.Role);
            });

            return CreatedAtAction(nameof(GetById),
                new { id = model.UserId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, User model)
        {
            if (id != model.UserId)
                return BadRequest("The URL ID must match UserId.");

            string query = @"UPDATE USERS SET
                FULL_NAME = @name, EMAIL = @email,
                PASSWORD_HASH = @password, ROLE = @role
                WHERE USER_ID = @id";

            int rows = db.Execute(query, c =>
            {
                c.Parameters.AddWithValue("@id", id);
                c.Parameters.AddWithValue("@name", model.FullName);
                c.Parameters.AddWithValue("@email", model.Email);
                c.Parameters.AddWithValue("@password", model.PasswordHash);
                c.Parameters.AddWithValue("@role", model.Role);
            });

            return rows == 0 ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            int rows = db.Execute(
                "DELETE FROM USERS WHERE USER_ID = @id",
                c => c.Parameters.AddWithValue("@id", id));

            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
