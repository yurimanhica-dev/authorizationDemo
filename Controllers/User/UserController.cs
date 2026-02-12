using System.Security.Claims;

using AuthorizationDemo.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers;

public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ==========================
    // LIST
    // ==========================
    [Authorize(Policy = "User.Index")]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            model.Add(new UserListViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }

        return View(model);
    }

    // ==========================
    // CREATE - GET
    // ==========================
    [Authorize(Policy = "User.Create")]
    public IActionResult Create()
    {
        var model = new UserViewModel();
        CarregarRoles(model);
        return View(model);
    }

    // ==========================
    // CREATE - POST
    // ==========================
    [HttpPost]
    public async Task<IActionResult> Create(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            CarregarRoles(model);
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email já está em uso.");
            CarregarRoles(model);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            CarregarRoles(model);
            return View(model);
        }

        await AtribuirRoles(user, model.Roles);

        return RedirectToAction(nameof(Index));
    }

    // ==========================
    // EDIT - POST
    // ==========================

    [HttpPost]
    public async Task<IActionResult> Edit(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            CarregarRoles(model);
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id!);
        if (user == null) return NotFound();

        // Atualiza dados básicos
        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError("", error.Description);

            CarregarRoles(model);
            return View(model);
        }

        // Atualiza senha, se fornecida
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    ModelState.AddModelError("", error.Description);

                CarregarRoles(model);
                return View(model);
            }

            var addResult = await _userManager.AddPasswordAsync(user, model.Password);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                    ModelState.AddModelError("", error.Description);

                CarregarRoles(model);
                return View(model);
            }
        }

        // Atualiza roles
        if (model.Roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await AtribuirRoles(user, model.Roles);
        }

        // Tudo certo, redireciona para Index
        return RedirectToAction(nameof(Index));
    }

    // ==========================
    // EDIT - GET
    // ==========================
    [Authorize(Policy = "User.Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Lista de roles que o usuário já possui
        var userRoles = await _userManager.GetRolesAsync(user);

        // Materializa todas as roles do sistema
        var roles = _roleManager.Roles.ToList();

        // Monta o ViewModel
        var model = new UserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles
                .Select(r => new RoleItem
                {
                    RoleName = r.Name!,
                    IsSelected = userRoles.Contains(r.Name!) // agora seguro
                })
                .ToList()
        };

        return View(model);
    }

    // ==========================
    // DELETE
    // ==========================
    [Authorize(Policy = "User.Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }

    private void CarregarRoles(UserViewModel model)
    {
        if (model.Roles != null)
        {
            bool hasMatchingRole = model.Roles.Any(x => x != null && x.RoleName == (_ = _roleManager.Roles.FirstOrDefault(r => r.Name == x.RoleName))?.Name && x.IsSelected);
            model.Roles = _roleManager.Roles
                .Select(r => new RoleItem
                {
                    RoleName = r.Name!,
                    IsSelected = hasMatchingRole
                })
                .ToList();
        }
    }

    private async Task AtribuirRoles(ApplicationUser user, List<RoleItem> roles)
    {
        var selectedRoles = roles
            .Where(r => r.IsSelected)
            .Select(r => r.RoleName);

        if (selectedRoles.Any())
            await _userManager.AddToRolesAsync(user, selectedRoles);
    }
}
