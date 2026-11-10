using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Models;
using RaceDayMvc.Models.Api;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(RaceDayApiClient api) : base(api)
        {
        }

        // GET: /
        // Visitors see the landing page, participants see upcoming events,
        // and organisers are sent to their dashboard.
        public async Task<IActionResult> Index(
            string? search,
            string? eventType,
            string? distance)
        {
            string? role = HttpContext.Session.GetString("Role");

            if (role == null)
            {
                return View("Landing");
            }

            if (role == "Organiser")
            {
                return RedirectToAction("Dashboard", "Organiser");
            }

            HttpContext.Response.Headers.CacheControl = "no-store, no-cache";

            // Turn the distance choice into the range the API understands.
            (decimal? min, decimal? max) = distance switch
            {
                "short" => ((decimal?)null, (decimal?)5m),
                "medium" => (5.01m, 10m),
                "long" => (10.01m, 21.1m),
                "ultra" => (21.11m, null),
                _ => (null, null)
            };

            var query = new List<string> { "upcomingOnly=true" };

            if (!string.IsNullOrWhiteSpace(search))
                query.Add("search=" + Uri.EscapeDataString(search.Trim()));
            if (!string.IsNullOrWhiteSpace(eventType))
                query.Add("eventType=" + Uri.EscapeDataString(eventType));
            if (min.HasValue)
                query.Add("minDistance=" + min.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (max.HasValue)
                query.Add("maxDistance=" + max.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

            var result = await Api.GetAsync<List<EventDto>>("api/Events?" + string.Join("&", query));

            if (result.IsUnauthorized)
            {
                return SessionExpired();
            }

            var model = new EventBrowseViewModel
            {
                Search = search,
                EventType = eventType,
                Distance = distance,
                Events = result.Data ?? new List<EventDto>()
            };

            if (!result.Success)
            {
                ViewBag.LoadError = result.Message;
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code = null)
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            if (code == 404)
            {
                model.Message = "We could not find that page.";
            }

            return View(model);
        }
    }
}