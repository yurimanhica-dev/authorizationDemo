using System.Security.Claims;

using AuthorizationDemo.Controllers;
using AuthorizationDemo.Data;
using AuthorizationDemo.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class RoleController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public RoleController(RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    [Authorize(Policy = "Role.Index")]
    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();
        return View(roles);
    }

    [Authorize(Policy = "Role.Create")]
    // GET: exibe o form
    public IActionResult Create()
    {
        var model = new RoleViewModel
        {
            Permissions = _context.Permissions
                .Select(p => new PermissionCheckbox
                {
                    PermissionId = p.Id,
                    Name = p.Name,
                    IsSelected = false
                })
                .ToList()
        };
        return View(model);
    }

    // POST: cria a role + adiciona claims
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        // if (!ModelState.IsValid)
        //     return View(model);

        if (await _roleManager.RoleExistsAsync(model.RoleName))
        {
            ModelState.AddModelError("", "Role já existe");
            return View(model);
        }

        var role = new IdentityRole
        {
            Name = model.RoleName,
            NormalizedName = model.RoleName.ToUpper()
        };

        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        var selectedPermissions = model.Permissions?
            .Where(p => p.IsSelected)
            .ToList() ?? new();

        foreach (var permission in selectedPermissions)
        {
            var claimResult = await _roleManager.AddClaimAsync(
                role,
                new Claim("Permission", permission.Name)
            );

            if (!claimResult.Succeeded)
            {
                foreach (var error in claimResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }
        }

        TempData["SuccessMessage"] = "Role criada com sucesso";
        return RedirectToAction(nameof(Index));
    }


    [Authorize(Policy = "Role.Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        await _roleManager.DeleteAsync(role);
        return RedirectToAction("Index", "Role");
    }

    [Authorize(Policy = "Role.Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        // pega todas as claims do role primeiro
        var roleClaims = await _roleManager.GetClaimsAsync(role);

        // pega todas as permissões da tabela
        var permissions = await _context.Permissions
            .AsNoTracking()
            .ToListAsync();

        // monta o model
        var model = new RoleViewModel
        {
            Id = role.Id,
            RoleName = role.Name!,
            Permissions = permissions.Select(p => new PermissionCheckbox
            {
                PermissionId = p.Id,
                Name = p.Name,
                IsSelected = roleClaims.Any(c => c.Type == "Permission" && c.Value == p.Name)
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(RoleViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var role = await _roleManager.FindByIdAsync(model.Id!);
        if (role == null) return NotFound();

        // Atualiza nome da role
        role.Name = model.RoleName;
        var updateResult = await _roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // Busca claims existentes
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var claimsToRemove = existingClaims.Where(c => c.Type == "Permission").ToList();

        // Remove claims antigas de forma sequencial
        foreach (var claim in claimsToRemove)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Adiciona novas claims de forma sequencial
        var permissionsToAdd = model.Permissions.Where(p => p.IsSelected).ToList();
        foreach (var permission in permissionsToAdd)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission.Name));
        }

        return RedirectToAction("Index", "Role");
    }
}
