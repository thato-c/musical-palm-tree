using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineCampus.Interfaces;

namespace OnlineCampus.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository) 
        { 
            _roleRepository = roleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!await _roleRepository.CreateRoleAsync(roleName))
            {
                ModelState.AddModelError("", "Role creation failed.");
                return View();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleRepository.FindRoleByIdAsync(roleId);
            if (role == null) return NotFound();
            
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteRole(string roleId)
        {
            if (!await _roleRepository.DeleteRoleAsync(roleId))
            {
                ModelState.AddModelError("", "Role deletion failed.");
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}
