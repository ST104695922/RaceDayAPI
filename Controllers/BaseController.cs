using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    // Shared behaviour for every controller that calls the API.
    public abstract class BaseController : Controller
    {
        protected readonly RaceDayApiClient Api;

        protected BaseController(RaceDayApiClient api)
        {
            Api = api;
        }

        // Ends the MVC session and returns to the login page.
        // Used when the API says the user's session has expired.
        protected IActionResult SessionExpired()
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        // Shows an API problem on the next page as an alert.
        protected void SetError(string message)
        {
            TempData["Error"] = message;
        }

        protected void SetSuccess(string message)
        {
            TempData["Success"] = message;
        }
    }
}