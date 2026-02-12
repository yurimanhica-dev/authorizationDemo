using AuthorizationDemo.Data;
using AuthorizationDemo.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationDemo.Controllers
{
    public class PermissionController : Controller
    {
        private readonly AppDbContext _context;
        public PermissionController(AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
        }

        [Authorize(Policy = "Permission.Index")]
        public IActionResult Index()
        {
            var permissions = _context.Permissions.ToList();
            return View(permissions);
        }

        [Authorize(Policy = "Permission.Create")]
        // GET: criar permission
        public IActionResult Create()
        {
            return View();
        }

        // POST: criar permission
        [HttpPost]
        public async Task<IActionResult> Create(PermissionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var permission = new PermissionViewModel
            {
                Name = model.Name,
                Description = model.Description
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return RedirectToAction("Create");
        }

        [Authorize(Policy = "Permission.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Policy = "Permission.Edit")]
        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            // Se usar ViewModel, mapeia
            var model = new PermissionViewModel
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PermissionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var permission = await _context.Permissions.FindAsync(model.Id);
            if (permission == null) return NotFound();

            // Atualiza os campos
            permission.Name = model.Name;
            permission.Description = model.Description;

            // Salva no banco
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
