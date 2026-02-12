using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers
{
    public class EmployeeController : Controller
    {
        // [Authorize(Policy = "Employee.Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "Employee.Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "Employee.Edit")]
        public IActionResult Edit(string id)
        {
            return View();
        }

        [Authorize(Policy = "Employee.Delete")]
        public IActionResult Delete(string id)
        {
            return View();
        }
    }
}
