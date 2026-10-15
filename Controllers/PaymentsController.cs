
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class PaymentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public PaymentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Payments
        // Participants see their own payments.
        // Organisers see payments for their events.
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                var enrolmentIds = await _context.Enrolments
                    .Where(x => x.ParticipantId == userId)
                    .Select(x => x.EnrolmentId)
                    .ToListAsync();

                var payments = await _context.Payments
                    .AsNoTracking()
                    .Where(x => enrolmentIds.Contains(x.EnrolmentId))
                    .ToListAsync();

                return Ok(payments);
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

                var enrolmentIds = await _context.Enrolments
                    .Where(x => categoryIds.Contains(x.CategoryId))
                    .Select(x => x.EnrolmentId)
                    .ToListAsync();

                var payments = await _context.Payments
                    .AsNoTracking()
                    .Where(x => enrolmentIds.Contains(x.EnrolmentId))
                    .ToListAsync();

                return Ok(payments);
            }

            return Forbid();
        }

        // GET: api/Payments/P0001
        // A user can only view a payment they are authorised to access.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(string id)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PaymentId == id);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "Payment not found."
                });
            }

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.EnrolmentId == payment.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Related enrolment not found."
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

                return Ok(payment);
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

                return Ok(payment);
            }

            return Forbid();
        }
    }
}
