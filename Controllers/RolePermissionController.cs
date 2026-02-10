using System.Security.Claims;

using AuthorizationDemo.Data;
using AuthorizationDemo.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers
{
    public class RolePermissionController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public RolePermissionController(RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // [Authorize(Policy = "RolePermission.Index")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // [Authorize(Policy = "RolePermission.Create")]
        public IActionResult Create()
        {
            var model = new RolePermissionsViewModel
            {
                Permissions = _context.Permissions.Select(p => new PermissionCheckbox
                {
                    PermissionId = p.Id,
                    Name = p.Name,
                    IsSelected = false
                }).ToList()
            };

            return View(model);
        }

        // [Authorize(Policy = "RolePermission.Edit")]
        // GET: mostrar role e checkboxes de permissões
        public async Task<IActionResult> Edit(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var allPermissions = _context.Permissions.ToList();
            var roleClaims = await _roleManager.GetClaimsAsync(role);

            var model = new RolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permissions = allPermissions.Select(p => new PermissionCheckbox
                {
                    PermissionId = p.Id,
                    Name = p.Name,
                    IsSelected = roleClaims.Any(c => c.Type == "Permission" && c.Value == p.Name)
                }).ToList()
            };

            return View(model);
        }

        // POST: salvar mudanças
        [HttpPost]
        public async Task<IActionResult> Edit(RolePermissionsViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return NotFound();

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            foreach (var permission in model.Permissions.Where(p => p.IsSelected))
            {
                if (string.IsNullOrWhiteSpace(permission.Name))
                    continue;

                var result = await _roleManager.AddClaimAsync(
                    role,
                    new Claim("Permission", permission.Name)
                );

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("",
                        $"Erro ao adicionar {permission.Name}");
                }
            }

            // if (!ModelState.IsValid)
            //     return View(model);

            return RedirectToAction("Index", "RolePermission");
        }
    }
}
