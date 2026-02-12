using AuthorizationDemo.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ======================
        // LOGIN - GET
        // ======================
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // ======================
        // LOGIN - POST
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada temporariamente");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos");
            }

            return View(model);
        }

        // ======================
        // LOGOUT
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // ======================
        // ACCESS DENIED
        // ======================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
