using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Models;
using RaceDayMvc.Models.Api;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(RaceDayApiClient api) : base(api)
        {
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Users who are already logged in go straight to their home page.
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToHome();
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The API checks the password and creates its own session.
            var result = await Api.PostAsync<LoginResponse>("api/Auth/login", new
            {
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty,
                    result.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Invalid email, password, or account type."
                        : result.Message);
                return View(model);
            }

            // Remember who the user is and which role they have on every page.
            HttpContext.Session.SetString("UserId", result.Data.UserId);
            HttpContext.Session.SetString("Role", result.Data.Role);
            HttpContext.Session.SetString("FullName", result.Data.FullName);

            // Only follow return URLs that stay inside this website.
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToHome();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToHome();
            }

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await Api.PostAsync<object>("api/Auth/register", new
            {
                fullName = model.FullName,
                email = model.Email,
                password = model.Password,
                role = model.Role
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            SetSuccess("Your account has been created. Please log in.");
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // End the API session first, then clear the MVC session.
            if (HttpContext.Session.GetString("UserId") != null)
            {
                await Api.PostAsync<object>("api/Auth/logout");
            }

            HttpContext.Session.Clear();
            SetSuccess("You have been logged out.");
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Sends each role to its own home page.
        private IActionResult RedirectToHome()
        {
            return HttpContext.Session.GetString("Role") == "Organiser"
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
        }
    }
}