using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            bool isAuthenticated = User?.Identity?.IsAuthenticated ?? false;

            return statusCode switch
            {
                404 => isAuthenticated
                    ? View("NotFoundPage")
                    : RedirectToAction("Login", "Account"),

                403 => isAuthenticated
                    ? View("AccessDenied")
                    : RedirectToAction("Login", "Account"),

                _ => View("GenericError")
            };
        }
    }
}
