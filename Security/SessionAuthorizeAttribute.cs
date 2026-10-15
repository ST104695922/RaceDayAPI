
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayAPI.Security
{
    // Checks whether a user has an active session
    // and, when specified, whether their role is permitted.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _allowedRoles;

        public SessionAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;

            var userId = session.GetString("UserId");
            var role = session.GetString("Role");

            // Reject requests without an authenticated session.
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(role))
            {
                context.Result = new UnauthorizedObjectResult(
                    new { message = "Please log in first." });

                return;
            }

            // Reject users whose role is not permitted.
            if (_allowedRoles.Length > 0 &&
                !_allowedRoles.Contains(
                    role, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(
                    new { message = "You do not have permission." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return;
            }

            await next();
        }
    }
}
