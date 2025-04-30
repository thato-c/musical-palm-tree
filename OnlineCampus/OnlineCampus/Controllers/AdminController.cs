using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.Repositories;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private IAdminRepository _adminRepository;
        private IAuthRepository _authRepository;

        public AdminController(ILogger<AdminController> logger, IAdminRepository adminRepository, IAuthRepository authrepository)
        {
            _logger = logger;
            _adminRepository = adminRepository;
            _authRepository = authrepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;            
            var admins = from a in _adminRepository.GetAdmins() select a;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                admins = admins.Where(a => a.LastName.Contains(searchString) || a.FirstName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    admins = admins.OrderByDescending(a => a.LastName);
                    break;
                default:
                    admins = admins.OrderBy(a => a.LastName);
                    break;
            }
            int pageSize = 8;
            return View(await PaginatedList<Admin>.CreateAsync((IQueryable<Admin>)admins, pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            var roles = new[] { "Admin", "SuperAdmin" };

            var viewModel = new AdminViewModel
            {
                RoleOptions = new SelectList(roles)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminViewModel viewModel)
        {
            if (ModelState.IsValid)
            {

                var user = new User
                {
                    FirstName = viewModel.FirstName,
                    LastName = viewModel.LastName,
                    Email = viewModel.Email,
                    UserName = viewModel.UserName,
                };

                var result = await _authRepository.RegisterUserAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    await _authRepository.AssignRoleAsync(user, viewModel.SelectedRole);

                    // Map the ViewModel to the Admin Model
                    var admin = new Models.Admin
                    {
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                        UserId = user.Id,
                    };

                    _adminRepository.InsertAdmin(admin);
                    await _adminRepository.SaveAsync();

                    await _authRepository.SendConfirmationEmailAsync(user, Url.Content("~/"));

                    return View("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(viewModel);
            }
            viewModel.RoleOptions = new SelectList(new[] { "Admin", "SuperAdmin" });
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid AdminId)
        {
            var admin = await _adminRepository.GetAdminByIdAsync(AdminId);

            if (admin == null)
            {
                TempData["Message"] = "The Admin has not been found.";
                return View();
            }

            var viewModel = new AdminDetailViewModel
            {
                AdminId = admin.AdminId,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                RowVersion = admin.RowVersion,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var adminToEdit = await _adminRepository.GetAdminByIdAsync(viewModel.AdminId);

                if (adminToEdit != null)
                {
                    if (adminToEdit.FirstName == viewModel.FirstName && adminToEdit.LastName == viewModel.LastName)
                    {
                        ModelState.AddModelError(string.Empty, "Data has not been modified");
                        return View(viewModel);
                    }
                    else
                    {
                        _adminRepository.SetOriginalRowVersion(adminToEdit, viewModel.RowVersion);

                        adminToEdit.FirstName = viewModel.FirstName;
                        adminToEdit.LastName = viewModel.LastName;

                        try
                        {
                            _adminRepository.UpdateAdmin(adminToEdit);
                            await _adminRepository.SaveAsync();
                            return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            _logger.LogError(ex, "Concurrency error while updating admin.");
                            ModelState.AddModelError("", "Concurrency error. Please try again.");
                        }

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["Message"] = "Admin was not found";
                    return View();
                }
            }
            else
            {
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid AdminId)
        {
            var admin = await _adminRepository.GetAdminByIdAsync(AdminId);

            if (admin == null)
            {
                TempData["Message"] = "The Admin has not been found.";
                return View();
            }

            var viewModel = new AdminDetailViewModel
            {
                AdminId = admin.AdminId,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                RowVersion = admin.RowVersion,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(AdminDetailViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var adminToDelete = await _adminRepository.GetAdminByIdAsync(viewModel.AdminId);

                if (adminToDelete == null)
                {
                    TempData["Message"] = "Admin was not found.";
                    return View();
                }

                try
                {
                    await _adminRepository.DeleteAdmin(viewModel.AdminId);
                    await _adminRepository.SaveAsync();
                    return RedirectToAction("Index");
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    var ExceptionEntry = ex.Entries.Single();
                    var databaseEntry = ExceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Unable to save the changes. The admin has been deleted by another user.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Concurrency error occurred");
                    }
                    return View(viewModel);
                }
            }
            else
            {
                return View(viewModel);
            }
        }
    }
}
