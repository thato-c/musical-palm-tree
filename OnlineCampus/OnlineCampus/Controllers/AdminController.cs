using Microsoft.AspNetCore.Mvc;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.Repositories;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Controllers
{
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
                    admins.OrderBy(a => a.LastName);
                    break;
            }
            int pageSize = 8;
            return View(await PaginatedList<Admin>.CreateAsync((IQueryable<Admin>)admins, pageNumber ?? 1, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
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
                    // Map the ViewModel to the Admin Model
                    var Admin = new Models.Admin
                    {
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                        UserId = user.Id,
                    };

                    _adminRepository.InsertAdmin(Admin);
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
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid AdminId)
        {
            var admin = await _adminRepository.GetAdminByIdAsync(AdminId);

            if (admin == null)
            {
                ViewBag.Message = "The Admin has not been found.";
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

        [HttpGet]
        public async Task<IActionResult> Delete(Guid AdminId)
        {
            var admin = await _adminRepository.GetAdminByIdAsync(AdminId);

            if (admin == null)
            {
                ViewBag.Message = "The Admin has not been found.";
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
    }
}
