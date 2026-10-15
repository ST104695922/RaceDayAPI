using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Both participants and organisers can view all events.
        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/E0001
        // View one event by its ID.
        [HttpGet("{id}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetEvent(string id)
        {
            var eventDetails = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventDetails == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(eventDetails);
        }

        // POST: api/Events
        // Only an organiser can create an event.
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> CreateEvent(Event eventDetails)
        {
            if (string.IsNullOrWhiteSpace(eventDetails.EventName) ||
                string.IsNullOrWhiteSpace(eventDetails.Location) ||
                eventDetails.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "Event name, date, and location are required."
                });
            }

            // Get the organiser ID from the logged-in session.
            var organiserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(organiserId))
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            // Confirm that this organiser exists.
            var organiserExists = await _context.Organisers
                .AnyAsync(o => o.OrganiserId == organiserId);

            if (!organiserExists)
            {
                return Forbid();
            }

            // Generate the next event ID because the database does not use IDENTITY.
            var existingIds = await _context.Events
                .Select(e => e.EventId)
                .ToListAsync();

            var highestNumber = existingIds
                .Where(id => id.Length == 5 &&
                             id.StartsWith("E") &&
                             int.TryParse(id.Substring(1), out _))
                .Select(id => int.Parse(id.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            eventDetails.EventId = $"E{highestNumber + 1:D4}";
            eventDetails.OrganiserId = organiserId;
            eventDetails.Status = string.IsNullOrWhiteSpace(eventDetails.Status)
                ? "Upcoming"
                : eventDetails.Status;

            _context.Events.Add(eventDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventDetails.EventId },
                eventDetails);
        }

        // PUT: api/Events/E0001
        // An organiser can update only their own events.
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEvent(
            string id,
            Event updatedEvent)
        {
            var organiserId = HttpContext.Session.GetString("UserId");

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (existingEvent.OrganiserId != organiserId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(updatedEvent.EventName) ||
                string.IsNullOrWhiteSpace(updatedEvent.Location) ||
                updatedEvent.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "Event name, date, and location are required."
                });
            }

            existingEvent.EventName = updatedEvent.EventName;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location;

            if (!string.IsNullOrWhiteSpace(updatedEvent.Status))
            {
                existingEvent.Status = updatedEvent.Status;
            }

            // Keep the original event ID and organiser ID.
            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/Events/E0001
        // An organiser can delete only their own events.
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            var organiserId = HttpContext.Session.GetString("UserId");

            var eventDetails = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventDetails == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventDetails.OrganiserId != organiserId)
            {
                return Forbid();
            }

            // Prevent deletion if categories still reference this event.
            var hasCategories = await _context.Categories
                .AnyAsync(c => c.EventId == id);

            if (hasCategories)
            {
                return Conflict(new
                {
                    message = "This event has categories. Remove those categories before deleting the event."
                });
            }

            _context.Events.Remove(eventDetails);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}