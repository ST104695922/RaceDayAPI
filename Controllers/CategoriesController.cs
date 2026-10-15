
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDayAPI.Database;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDataAccess db = new RaceDayDataAccess();

        private Category Map(SqlDataReader r) => new Category
        {
            CategoryId = r.GetString(0),
            EventId = r.GetString(1),
            CategoryName = r.GetString(2),
            DistanceKm = r.GetDecimal(3),
            EntryFee = r.GetDecimal(4)
        };

        private const string SelectColumns =
            "CATEGORY_ID, EVENT_ID, CATEGORY_NAME, DISTANCE_KM, ENTRY_FEE";

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(db.GetAll($"SELECT {SelectColumns} FROM CATEGORY", Map));

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var record = db.GetById(
                $"SELECT {SelectColumns} FROM CATEGORY WHERE CATEGORY_ID = @id",
                id, Map);

            return record == null ? NotFound() : Ok(record);
        }

        [HttpPost]
        public IActionResult Create(Category model)
        {
            if (string.IsNullOrWhiteSpace(model.CategoryId) ||
                string.IsNullOrWhiteSpace(model.EventId) ||
                string.IsNullOrWhiteSpace(model.CategoryName))
                return BadRequest("Required category fields must be supplied.");

            if (model.CategoryId.Length != 5)
                return BadRequest("CategoryId must contain exactly 5 characters.");

            if (model.DistanceKm <= 0 || model.EntryFee < 0)
                return BadRequest("Distance must be positive and fee cannot be negative.");

            string query = @"INSERT INTO CATEGORY
                (CATEGORY_ID, EVENT_ID, CATEGORY_NAME, DISTANCE_KM, ENTRY_FEE)
                VALUES (@id, @event, @name, @distance, @fee)";

            db.Execute(query, c =>
            {
                c.Parameters.AddWithValue("@id", model.CategoryId);
                c.Parameters.AddWithValue("@event", model.EventId);
                c.Parameters.AddWithValue("@name", model.CategoryName);
                c.Parameters.AddWithValue("@distance", model.DistanceKm);
                c.Parameters.AddWithValue("@fee", model.EntryFee);
            });

            return CreatedAtAction(nameof(GetById),
                new { id = model.CategoryId }, model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, Category model)
        {
            if (id != model.CategoryId)
                return BadRequest("The URL ID must match CategoryId.");

            string query = @"UPDATE CATEGORY SET
                EVENT_ID = @event, CATEGORY_NAME = @name,
                DISTANCE_KM = @distance, ENTRY_FEE = @fee
                WHERE CATEGORY_ID = @id";

            int rows = db.Execute(query, c =>
            {
                c.Parameters.AddWithValue("@id", id);
                c.Parameters.AddWithValue("@event", model.EventId);
                c.Parameters.AddWithValue("@name", model.CategoryName);
                c.Parameters.AddWithValue("@distance", model.DistanceKm);
                c.Parameters.AddWithValue("@fee", model.EntryFee);
            });

            return rows == 0 ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            int rows = db.Execute(
                "DELETE FROM CATEGORY WHERE CATEGORY_ID = @id",
                c => c.Parameters.AddWithValue("@id", id));

            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
