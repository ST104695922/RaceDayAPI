
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
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ProfileController(RaceDayContext context)
        {
            _context = context;
        }

        // View the profile belonging to the logged-in user.
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            string? userId = HttpContext.Session.GetString("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (role == "Participant")
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound(new { message = "Profile not found." });

                return Ok(new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    Role = user.Role
                });
            }

            if (role == "Organiser")
            {
                var organiser = await _context.Organisers
                    .AsNoTracking()
                    .SingleOrDefaultAsync(o => o.OrganiserId == userId);

                if (organiser == null)
                    return NotFound(new { message = "Profile not found." });

                return Ok(new
                {
                    organiser.OrganiserId,
                    organiser.FullName,
                    organiser.Email,
                    Role = "Organiser"
                });
            }

            return Unauthorized(new { message = "Please log in again." });
        }

        // Update the name and email of the logged-in user.
        // The user cannot change their own role or ID.
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? userId = HttpContext.Session.GetString("UserId");
            string? role = HttpContext.Session.GetString("Role");

            string email = request.Email.Trim();
            string fullName = request.FullName.Trim();

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new
                {
                    message = "Name and email are required."
                });
            }

            // Handle Participant profiles.
            if (role == "Participant")
            {
                var user = await _context.Users
                    .SingleOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound(new { message = "Profile not found." });

                bool emailUsedByAnotherAccount =
                    await _context.Users.AnyAsync(
                        u => u.Email == email && u.UserId != userId)
                    ||
                    await _context.Organisers.AnyAsync(
                        o => o.Email == email);

                if (emailUsedByAnotherAccount)
                    return Conflict(new
                    {
                        message = "That email address is already in use."
                    });

                user.FullName = fullName;
                user.Email = email;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile updated successfully.",
                    user.UserId,
                    user.FullName,
                    user.Email,
                    Role = user.Role
                });
            }

            // Handle Organiser profiles.
            if (role == "Organiser")
            {
                var organiser = await _context.Organisers
                    .SingleOrDefaultAsync(o => o.OrganiserId == userId);

                if (organiser == null)
                    return NotFound(new { message = "Profile not found." });

                bool emailUsedByAnotherAccount =
                    await _context.Organisers.AnyAsync(
                        o => o.Email == email && o.OrganiserId != userId)
                    ||
                    await _context.Users.AnyAsync(
                        u => u.Email == email);

                if (emailUsedByAnotherAccount)
                    return Conflict(new
                    {
                        message = "That email address is already in use."
                    });

                organiser.FullName = fullName;
                organiser.Email = email;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile updated successfully.",
                    organiser.OrganiserId,
                    organiser.FullName,
                    organiser.Email,
                    Role = "Organiser"
                });
            }

            return Unauthorized(new { message = "Please log in again." });
        }
    }
}
