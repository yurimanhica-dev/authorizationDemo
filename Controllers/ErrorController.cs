using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            return statusCode switch
            {
                404 => View("NotFoundPage"),
                403 => RedirectToAction("AccessDenied", "Account"),
                _ => View("GenericError")
            };
        }
    }
}
