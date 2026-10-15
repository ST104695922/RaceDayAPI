
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Security;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public AuthController(RaceDayContext context)
        {
            _context = context;
        }

        // Register a Participant or Organiser.
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string role = request.Role.Trim();

            if (role.Equals("Participant",
                    StringComparison.OrdinalIgnoreCase))
            {
                role = "Participant";
            }
            else if (role.Equals("Organiser",
                         StringComparison.OrdinalIgnoreCase))
            {
                role = "Organiser";
            }
            else
            {
                return BadRequest(new
                {
                    message = "Role must be Participant or Organiser."
                });
            }

            // Email addresses must be unique across both account tables.
            string email = request.Email.Trim();

            bool emailExists =
                await _context.Users.AnyAsync(u => u.Email == email) ||
                await _context.Organisers.AnyAsync(o => o.Email == email);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            if (role == "Participant")
            {
                var participant = new User
                {
                    UserId = await GenerateUserId(),
                    FullName = request.FullName.Trim(),
                    Email = email,
                    Role = "Participant"
                };

                // Hash the password; never store the plain-text password.
                var hasher = new PasswordHasher<User>();
                participant.PasswordHash =
                    hasher.HashPassword(participant, request.Password);

                _context.Users.Add(participant);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Participant registered successfully.",
                    userId = participant.UserId,
                    role = participant.Role
                });
            }
            else
            {
                var organiser = new Organiser
                {
                    OrganiserId = await GenerateOrganiserId(),
                    FullName = request.FullName.Trim(),
                    Email = email
                };

                var hasher = new PasswordHasher<Organiser>();
                organiser.PasswordHash =
                    hasher.HashPassword(organiser, request.Password);

                _context.Organisers.Add(organiser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Organiser registered successfully.",
                    userId = organiser.OrganiserId,
                    role = "Organiser"
                });
            }
        }

        // Log in using the account type, email, and password.
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string email = request.Email.Trim();
            string role = request.Role.Trim();

            string userId;
            string fullName;
            string actualRole;

            if (role.Equals("Participant",
                    StringComparison.OrdinalIgnoreCase))
            {
                var participant = await _context.Users
                    .SingleOrDefaultAsync(u => u.Email == email);

                if (participant == null)
                    return Unauthorized(new
                    {
                        message = "Invalid email, password, or role."
                    });

                var hasher = new PasswordHasher<User>();
                var verification = hasher.VerifyHashedPassword(
                    participant,
                    participant.PasswordHash,
                    request.Password);

                if (verification == PasswordVerificationResult.Failed)
                    return Unauthorized(new
                    {
                        message = "Invalid email, password, or role."
                    });

                userId = participant.UserId;
                fullName = participant.FullName;
                actualRole = "Participant";
            }
            else if (role.Equals("Organiser",
                         StringComparison.OrdinalIgnoreCase))
            {
                var organiser = await _context.Organisers
                    .SingleOrDefaultAsync(o => o.Email == email);

                if (organiser == null)
                    return Unauthorized(new
                    {
                        message = "Invalid email, password, or role."
                    });

                var hasher = new PasswordHasher<Organiser>();
                var verification = hasher.VerifyHashedPassword(
                    organiser,
                    organiser.PasswordHash,
                    request.Password);

                if (verification == PasswordVerificationResult.Failed)
                    return Unauthorized(new
                    {
                        message = "Invalid email, password, or role."
                    });

                userId = organiser.OrganiserId;
                fullName = organiser.FullName;
                actualRole = "Organiser";
            }
            else
            {
                return BadRequest(new
                {
                    message = "Role must be Participant or Organiser."
                });
            }

            // The server stores the verified identity and role in the session.
            HttpContext.Session.SetString("UserId", userId);
            HttpContext.Session.SetString("Role", actualRole);

            return Ok(new
            {
                message = "Login successful.",
                userId,
                fullName,
                role = actualRole
            });
        }

        // End the current session.
        [HttpPost("logout")]
        [SessionAuthorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok(new { message = "Logged out successfully." });
        }

        // Return the identity associated with the current session.
        [HttpGet("me")]
        [SessionAuthorize]
        public async Task<IActionResult> Me()
        {
            string? userId = HttpContext.Session.GetString("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                var participant = await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserId == userId);

                if (participant == null)
                {
                    HttpContext.Session.Clear();
                    return Unauthorized();
                }

                return Ok(new
                {
                    participant.UserId,
                    participant.FullName,
                    participant.Email,
                    Role = participant.Role
                });
            }

            if (role == "Organiser")
            {
                var organiser = await _context.Organisers
                    .AsNoTracking()
                    .SingleOrDefaultAsync(o => o.OrganiserId == userId);

                if (organiser == null)
                {
                    HttpContext.Session.Clear();
                    return Unauthorized();
                }

                return Ok(new
                {
                    organiser.OrganiserId,
                    organiser.FullName,
                    organiser.Email,
                    Role = "Organiser"
                });
            }

            HttpContext.Session.Clear();
            return Unauthorized();
        }

        // Generate IDs because the existing SQL tables do not use IDENTITY.
        private async Task<string> GenerateUserId()
        {
            var ids = await _context.Users
                .Select(u => u.UserId)
                .ToListAsync();

            int highest = ids
                .Select(id => int.TryParse(id.Substring(1), out int n)
                    ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            if (highest >= 9999)
                throw new InvalidOperationException(
                    "No more User IDs are available.");

            return $"U{highest + 1:0000}";
        }

        private async Task<string> GenerateOrganiserId()
        {
            var ids = await _context.Organisers
                .Select(o => o.OrganiserId)
                .ToListAsync();

            int highest = ids
                .Select(id => int.TryParse(id.Substring(1), out int n)
                    ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            if (highest >= 9999)
                throw new InvalidOperationException(
                    "No more Organiser IDs are available.");

            return $"O{highest + 1:0000}";
        }
    }
}
