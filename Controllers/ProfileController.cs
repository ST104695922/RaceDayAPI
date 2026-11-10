using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Models.Api;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    // View and update personal information and the profile picture.
    [RoleAuthorize]
    public class ProfileController : BaseController
    {
        public ProfileController(RaceDayApiClient api) : base(api)
        {
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await Api.GetAsync<ProfileDto>("api/Profile");

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                SetError(result.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(ToViewModel(result.Data));
        }

        // POST: /Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var result = await Api.PutAsync<ProfileDto>("api/Profile", new
            {
                fullName = model.FullName,
                email = model.Email
            });

            if (result.IsUnauthorized)
                return SessionExpired();

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("Index", model);
            }

            // Keep the name shown in the navigation bar up to date.
            HttpContext.Session.SetString("FullName", model.FullName.Trim());

            SetSuccess("Your profile has been updated.");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/UploadPicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {
            if (picture == null || picture.Length == 0)
            {
                SetError("Please choose a picture to upload.");
                return RedirectToAction(nameof(Index));
            }

            // The file goes to the API, which stores it in Azure Blob Storage.
            var result = await Api.PostFileAsync<object>("api/Profile/picture", picture);

            if (result.IsUnauthorized)
                return SessionExpired();

            if (result.Success)
                SetSuccess("Your profile picture has been updated.");
            else
                SetError(result.Message);

            return RedirectToAction(nameof(Index));
        }

        private static ProfileViewModel ToViewModel(ProfileDto dto)
        {
            return new ProfileViewModel
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role,
                ProfilePictureUrl = dto.ProfilePictureUrl
            };
        }
    }
}