
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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Participants see their own enrolments.
        // Organisers see enrolments for their events.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                var ownEnrolments = await _context.Enrolments
                    .AsNoTracking()
                    .Where(x => x.ParticipantId == userId)
                    .ToListAsync();

                return Ok(ownEnrolments);
            }

            if (role == "Organiser")
            {
                var eventIds = await _context.Events
                    .Where(x => x.OrganiserId == userId)
                    .Select(x => x.EventId)
                    .ToListAsync();

                var categoryIds = await _context.Categories
                    .Where(x => eventIds.Contains(x.EventId))
                    .Select(x => x.CategoryId)
                    .ToListAsync();

                var enrolments = await _context.Enrolments
                    .AsNoTracking()
                    .Where(x => categoryIds.Contains(x.CategoryId))
                    .ToListAsync();

                return Ok(enrolments);
            }

            return Forbid();
        }

        // POST: api/Enrolments
        // Only participants can enrol.
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> CreateEnrolment(
            CategoryEnrolmentRequest request)
        {
            var participantId =
                HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(request.CategoryId))
            {
                return BadRequest(new
                {
                    message = "A category ID is required."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(
                    x => x.CategoryId == request.CategoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(x =>
                    x.ParticipantId == participantId &&
                    x.CategoryId == request.CategoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }

            // Generate an enrolment ID because the table
            // does not use an IDENTITY column.
            var existingIds = await _context.Enrolments
                .Select(x => x.EnrolmentId)
                .ToListAsync();

            var highestNumber = existingIds
                .Where(id =>
                    id.Length == 5 &&
                    id.StartsWith("N") &&
                    int.TryParse(id.Substring(1), out _))
                .Select(id => int.Parse(id.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            var enrolment = new Enrolment
            {
                EnrolmentId = $"N{highestNumber + 1:D4}",
                ParticipantId = participantId!,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.Today,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(enrolment);
        }

        // GET: api/Enrolments/N0001
        // Users can only view enrolments they are authorised to access.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(string id)
        {
            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                if (enrolment.ParticipantId != userId)
                {
                    return Forbid();
                }

                return Ok(enrolment);
            }

            if (role == "Organiser")
            {
                var eventId = await _context.Categories
                    .Where(x => x.CategoryId == enrolment.CategoryId)
                    .Select(x => x.EventId)
                    .FirstOrDefaultAsync();

                var organiserId = await _context.Events
                    .Where(x => x.EventId == eventId)
                    .Select(x => x.OrganiserId)
                    .FirstOrDefaultAsync();

                if (organiserId != userId)
                {
                    return Forbid();
                }

                return Ok(enrolment);
            }

            return Forbid();
        }
    }

    // Request body for creating an enrolment.
    // The participant ID is taken from the session, not the request.
    public class CategoryEnrolmentRequest
    {
        public string CategoryId { get; set; } = string.Empty;
    }
}
