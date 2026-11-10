using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Models.Api;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    // Every page an organiser uses. All data comes from the API.
    [RoleAuthorize("Organiser")]
    public class OrganiserController : BaseController
    {
        public OrganiserController(RaceDayApiClient api) : base(api)
        {
        }

        // GET: /Organiser/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var result = await Api.GetAsync<List<OrganiserEventDto>>("api/Events/mine");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success)
                ViewBag.LoadError = result.Message;

            return View(new DashboardViewModel
            {
                Events = result.Data ?? new List<OrganiserEventDto>()
            });
        }

        // GET: /Organiser/Events
        public async Task<IActionResult> Events()
        {
            var result = await Api.GetAsync<List<OrganiserEventDto>>("api/Events/mine");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success)
                ViewBag.LoadError = result.Message;

            return View(result.Data ?? new List<OrganiserEventDto>());
        }

        // ---------- Create and edit events ----------

        // GET: /Organiser/CreateEvent
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View("EventForm", new EventFormViewModel
            {
                EventDate = DateTime.Today.AddDays(30)
            });
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("EventForm", model);

            var result = await Api.PostAsync<EventDto>("api/Events", ToRequest(model));

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            // The event exists now, so the banner can be sent for it.
            string? bannerProblem = await UploadBannerIfProvided(result.Data.EventId, model.Banner);

            if (bannerProblem == null)
            {
                SetSuccess("Event created.");
            }
            else
            {
                SetSuccess("Event created, but the banner image was not saved: " + bannerProblem +
                           " You can add it again by editing the event.");
            }

            return RedirectToAction(nameof(Events));
        }

        // GET: /Organiser/EditEvent/E0001
        [HttpGet]
        public async Task<IActionResult> EditEvent(string id)
        {
            var result = await Api.GetAsync<EventDto>($"api/Events/{Uri.EscapeDataString(id)}");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                SetError(result.Message);
                return RedirectToAction(nameof(Events));
            }

            var e = result.Data;

            return View("EventForm", new EventFormViewModel
            {
                EventId = e.EventId,
                EventName = e.EventName,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                Status = e.Status,
                ExistingBannerUrl = e.BannerUrl
            });
        }

        // POST: /Organiser/EditEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EventId))
                return RedirectToAction(nameof(Events));

            if (!ModelState.IsValid)
                return View("EventForm", model);

            var result = await Api.PutAsync<EventDto>(
                $"api/Events/{Uri.EscapeDataString(model.EventId)}", ToRequest(model));

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            string? bannerProblem = await UploadBannerIfProvided(model.EventId, model.Banner);

            if (bannerProblem == null)
                SetSuccess("Event updated.");
            else
                SetSuccess("Event updated, but the banner image was not saved: " + bannerProblem);

            return RedirectToAction(nameof(Events));
        }

        // POST: /Organiser/DeleteEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            var result = await Api.DeleteAsync<object>($"api/Events/{Uri.EscapeDataString(id)}");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
                SetSuccess("Event deleted.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Events));
        }

        // ---------- Categories ----------

        // GET: /Organiser/Categories/E0001
        [HttpGet]
        public async Task<IActionResult> Categories(string id)
        {
            var model = await BuildCategoriesModel(id);

            if (model == null)
                return RedirectToAction(nameof(Events));

            return View(model);
        }

        // POST: /Organiser/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(
            [Bind(Prefix = "NewCategory")] CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var invalid = await BuildCategoriesModel(form.EventId);

                if (invalid == null)
                    return RedirectToAction(nameof(Events));

                invalid.NewCategory = form;
                return View("Categories", invalid);
            }

            var result = await Api.PostAsync<CategoryDto>("api/Categories", new
            {
                eventId = form.EventId,
                categoryName = form.CategoryName.Trim(),
                distanceKm = form.DistanceKm,
                entryFee = form.EntryFee
            });

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
                SetSuccess("Category added.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Categories), new { id = form.EventId });
        }

        // POST: /Organiser/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(string eventId, string categoryId)
        {
            var result = await Api.DeleteAsync<object>(
                $"api/Categories/{Uri.EscapeDataString(categoryId)}");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
                SetSuccess("Category removed.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Categories), new { id = eventId });
        }

        // ---------- Enrolments ----------

        // GET: /Organiser/Enrolments/E0001
        [HttpGet]
        public async Task<IActionResult> Enrolments(string id)
        {
            var model = await BuildEnrolmentsModel(id);

            if (model == null)
                return RedirectToAction(nameof(Events));

            return View(model);
        }

        // POST: /Organiser/SetEnrolmentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEnrolmentStatus(
            string eventId, string enrolmentId, string status)
        {
            var result = await Api.PutAsync<object>(
                $"api/Enrolments/{Uri.EscapeDataString(enrolmentId)}/status", new { status });

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
                SetSuccess($"Enrolment marked as {status}.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }

        // ---------- Results ----------

        // GET: /Organiser/Results/E0001
        [HttpGet]
        public async Task<IActionResult> Results(string id)
        {
            var model = await BuildEnrolmentsModel(id);

            if (model == null)
                return RedirectToAction(nameof(Events));

            return View(model);
        }

        // POST: /Organiser/SaveResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(
            string eventId, string enrolmentId, string? resultId,
            string? finishTime, int? position)
        {
            // The API checks the rules; here we only turn the text box into a time.
            if (!TimeSpan.TryParseExact(finishTime?.Trim(), new[] { @"h\:mm\:ss", @"hh\:mm\:ss" },
                    System.Globalization.CultureInfo.InvariantCulture, out TimeSpan time))
            {
                SetError("Enter the finish time as hours:minutes:seconds, for example 01:45:30.");
                return RedirectToAction(nameof(Results), new { id = eventId });
            }

            ApiResult<object> result;

            if (string.IsNullOrWhiteSpace(resultId))
            {
                result = await Api.PostAsync<object>("api/Results", new
                {
                    enrolmentId,
                    finishTime = time,
                    position
                });
            }
            else
            {
                result = await Api.PutAsync<object>(
                    $"api/Results/{Uri.EscapeDataString(resultId)}", new
                    {
                        finishTime = time,
                        position
                    });
            }

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
                SetSuccess("Result saved.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        // POST: /Organiser/PublishResults
        // Marks the event as Completed so participants know the results are final.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishResults(string eventId)
        {
            var eventResult = await Api.GetAsync<EventDto>(
                $"api/Events/{Uri.EscapeDataString(eventId)}");

            if (eventResult.IsUnauthorized)
                return SessionExpired();

            if (!eventResult.Success || eventResult.Data == null)
            {
                SetError(eventResult.Message);
                return RedirectToAction(nameof(Events));
            }

            var e = eventResult.Data;

            var result = await Api.PutAsync<EventDto>(
                $"api/Events/{Uri.EscapeDataString(eventId)}", new
                {
                    eventName = e.EventName,
                    eventDate = e.EventDate,
                    location = e.Location,
                    description = e.Description,
                    distanceKm = e.DistanceKm,
                    eventType = e.EventType,
                    status = "Completed"
                });

            if (result.Success)
                SetSuccess("Results published. The event is now marked as Completed.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        // ---------- Helpers ----------

        // Builds the JSON the API expects for creating or updating an event.
        private static object ToRequest(EventFormViewModel model)
        {
            return new
            {
                eventName = model.EventName.Trim(),
                eventDate = model.EventDate,
                location = model.Location.Trim(),
                description = string.IsNullOrWhiteSpace(model.Description)
                    ? null : model.Description.Trim(),
                distanceKm = model.DistanceKm,
                eventType = model.EventType,
                status = string.IsNullOrWhiteSpace(model.Status) ? "Upcoming" : model.Status
            };
        }

        // Sends the banner to the API. Returns a message if it failed, otherwise null.
        private async Task<string?> UploadBannerIfProvided(string eventId, IFormFile? banner)
        {
            if (banner == null || banner.Length == 0)
                return null;

            var result = await Api.PostFileAsync<object>(
                $"api/Events/{Uri.EscapeDataString(eventId)}/banner", banner);

            return result.Success ? null : result.Message;
        }

        private async Task<CategoriesViewModel?> BuildCategoriesModel(string eventId)
        {
            var eventResult = await Api.GetAsync<EventDto>(
                $"api/Events/{Uri.EscapeDataString(eventId)}");

            if (!eventResult.Success || eventResult.Data == null)
            {
                SetError(eventResult.Message);
                return null;
            }

            var categories = await Api.GetAsync<List<CategoryDto>>(
                $"api/Categories?eventId={Uri.EscapeDataString(eventId)}");

            return new CategoriesViewModel
            {
                Event = eventResult.Data,
                Categories = categories.Data ?? new List<CategoryDto>(),
                NewCategory = new CategoryFormViewModel { EventId = eventId }
            };
        }

        private async Task<EventEnrolmentsViewModel?> BuildEnrolmentsModel(string eventId)
        {
            var eventResult = await Api.GetAsync<EventDto>(
                $"api/Events/{Uri.EscapeDataString(eventId)}");

            if (!eventResult.Success || eventResult.Data == null)
            {
                SetError(eventResult.Message);
                return null;
            }

            var enrolments = await Api.GetAsync<List<EnrolmentDto>>(
                $"api/Enrolments/event/{Uri.EscapeDataString(eventId)}");

            if (!enrolments.Success)
                ViewBag.LoadError = enrolments.Message;

            return new EventEnrolmentsViewModel
            {
                Event = eventResult.Data,
                Enrolments = enrolments.Data ?? new List<EnrolmentDto>()
            };
        }
    }
}