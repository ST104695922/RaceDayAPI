using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Models.Api;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    // Event pages for participants.
    [RoleAuthorize("Participant")]
    public class EventsController : BaseController
    {
        public EventsController(RaceDayApiClient api) : base(api)
        {
        }

        // GET: /Events/Details/E0001
        public async Task<IActionResult> Details(string id)
        {
            var eventResult = await Api.GetAsync<EventDto>($"api/Events/{Uri.EscapeDataString(id)}");

            if (eventResult.IsUnauthorized)
                return SessionExpired();

            if (!eventResult.Success || eventResult.Data == null)
            {
                SetError(eventResult.Message);
                return RedirectToAction("Index", "Home");
            }

            var categories = await Api.GetAsync<List<CategoryDto>>(
                $"api/Categories?eventId={Uri.EscapeDataString(id)}");

            var mine = await Api.GetAsync<List<EnrolmentDto>>("api/Enrolments/mine");

            var model = new EventDetailsViewModel
            {
                Event = eventResult.Data,
                Categories = categories.Data ?? new List<CategoryDto>(),
                EnrolledCategoryIds = (mine.Data ?? new List<EnrolmentDto>())
                    .Where(e => e.Status != "Cancelled")
                    .Select(e => e.CategoryId)
                    .ToHashSet()
            };

            return View(model);
        }

        // POST: /Events/Enrol
        // Records the enrolment in the system immediately through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(string eventId, string? categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                SetError("Please choose a category before entering the event.");
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var result = await Api.PostAsync<EnrolmentDto>("api/Enrolments", new { categoryId });

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
            {
                SetSuccess("You are entered! Your enrolment is pending confirmation from the organiser.");
                return RedirectToAction("MyEvents", "Participant");
            }

            SetError(result.Message);
            return RedirectToAction(nameof(Details), new { id = eventId });
        }
    }
}