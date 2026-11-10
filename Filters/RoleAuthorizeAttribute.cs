using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayMvc.Filters
{
    // Protects a controller or action using the session saved at login.
    // With no roles it only requires a logged-in user.
    // With roles (for example "Organiser") the user must also have one of them.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            string? userId = session.GetString("UserId");
            string? role = session.GetString("Role");

            // Pages that need a login must never be cached, otherwise the
            // browser's Back button could show them again after logging out.
            context.HttpContext.Response.Headers.CacheControl = "no-store, no-cache";
            context.HttpContext.Response.Headers.Pragma = "no-cache";

            // Not logged in: send the user to the login page.
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] = "Please log in to continue.";
                }

                string returnUrl = context.HttpContext.Request.Path +
                                   context.HttpContext.Request.QueryString;

                context.Result = new RedirectToActionResult(
                    "Login", "Account", new { returnUrl });
                return;
            }

            // Logged in with the wrong role: show the access denied page.
            if (_roles.Length > 0 &&
                !_roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied", "Account", null);
            }
        }
    }
}