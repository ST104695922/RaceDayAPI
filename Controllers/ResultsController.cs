
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Participants see their own results.
        // Organisers see results for their events.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                var enrolmentIds = await _context.Enrolments
                    .Where(x => x.ParticipantId == userId)
                    .Select(x => x.EnrolmentId)
                    .ToListAsync();

                var results = await _context.Results
                    .AsNoTracking()
                    .Where(x => enrolmentIds.Contains(x.EnrolmentId))
                    .ToListAsync();

                return Ok(results);
            }

            if (role == "Organiser")
            {
                var enrolmentIds = await (
                    from enrolment in _context.Enrolments
                    join category in _context.Categories
                        on enrolment.CategoryId equals category.CategoryId
                    join eventItem in _context.Events
                        on category.EventId equals eventItem.EventId
                    where eventItem.OrganiserId == userId
                    select enrolment.EnrolmentId
                ).ToListAsync();

                var results = await _context.Results
                    .AsNoTracking()
                    .Where(x => enrolmentIds.Contains(x.EnrolmentId))
                    .ToListAsync();

                return Ok(results);
            }

            return Forbid();
        }

        // POST: api/Results
        // Organisers can record results for their own events.
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateResult(ResultRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EnrolmentId))
            {
                return BadRequest(new
                {
                    message = "An enrolment ID is required."
                });
            }

            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishTime >= TimeSpan.FromDays(1))
            {
                return BadRequest(new
                {
                    message = "Enter a valid finish time."
                });
            }

            if (request.Position.HasValue && request.Position.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(
                    x => x.EnrolmentId == request.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(
                    x => x.CategoryId == enrolment.CategoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(
                    x => x.EventId == category.EventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var organiserId =
                HttpContext.Session.GetString("UserId");

            if (eventItem.OrganiserId != organiserId)
            {
                return Forbid();
            }

            var resultExists = await _context.Results
                .AnyAsync(x => x.EnrolmentId == request.EnrolmentId);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment. Use PUT to update it."
                });
            }

            var existingIds = await _context.Results
                .Select(x => x.ResultId)
                .ToListAsync();

            var highestNumber = existingIds
                .Where(id =>
                    id.Length == 5 &&
                    id.StartsWith("R") &&
                    int.TryParse(id.Substring(1), out _))
                .Select(id => int.Parse(id.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            var result = new Result
            {
                ResultId = $"R{highestNumber + 1:D4}",
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResults),
                new { id = result.ResultId },
                result);
        }

        // PUT: api/Results/R0001
        // Organisers can update results belonging to their events.
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateResult(
            string id,
            ResultUpdateRequest request)
        {
            var result = await _context.Results
                .FirstOrDefaultAsync(x => x.ResultId == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishTime >= TimeSpan.FromDays(1))
            {
                return BadRequest(new
                {
                    message = "Enter a valid finish time."
                });
            }

            if (request.Position.HasValue && request.Position.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(
                    x => x.EnrolmentId == result.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(
                    x => x.CategoryId == enrolment.CategoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(
                    x => x.EventId == category.EventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var organiserId =
                HttpContext.Session.GetString("UserId");

            if (eventItem.OrganiserId != organiserId)
            {
                return Forbid();
            }

            result.FinishTime = request.FinishTime;
            result.Position = request.Position;

            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }

    // Data required when recording a result.
    public class ResultRequest
    {
        public string EnrolmentId { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
    }

    // Data allowed when updating a result.
    public class ResultUpdateRequest
    {
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
    }
}
