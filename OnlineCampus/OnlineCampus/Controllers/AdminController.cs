using Microsoft.AspNetCore.Mvc;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private IAdminRepository _adminRepository;

        public AdminController(ILogger<AdminController> logger, IAdminRepository adminRepository)
        {
            _logger = logger;
            _adminRepository = adminRepository;
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
    }
}
