using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models.Api;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    // My Events and My Results pages for participants.
    [RoleAuthorize("Participant")]
    public class ParticipantController : BaseController
    {
        public ParticipantController(RaceDayApiClient api) : base(api)
        {
        }

        // GET: /Participant/MyEvents
        public async Task<IActionResult> MyEvents()
        {
            var result = await Api.GetAsync<List<EnrolmentDto>>("api/Enrolments/mine");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success)
                ViewBag.LoadError = result.Message;

            return View(result.Data ?? new List<EnrolmentDto>());
        }

        // GET: /Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            var result = await Api.GetAsync<List<EnrolmentDto>>("api/Enrolments/mine");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success)
                ViewBag.LoadError = result.Message;

            return View(result.Data ?? new List<EnrolmentDto>());
        }
    }
}