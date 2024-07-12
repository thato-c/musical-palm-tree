using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Controllers
{
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;
        private ICourseRepository courseRepository;

        public CourseController(ILogger<CourseController> logger, ICourseRepository courseRepository)
        {
            _logger = logger;
            this.courseRepository = courseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            try
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
                var courses = from c in courseRepository.GetCourses() 
                              select c;

                if (!String.IsNullOrEmpty(searchString) )
                {
                    courses = courses.Where(c => c.Name.Contains(searchString) 
                                              || c.Code.Contains(searchString));
                }
                switch (sortOrder)
                {
                    case "name_desc":
                        courses = courses.OrderByDescending(c => c.Name);
                        break;
                    default:
                        courses.OrderBy(c => c.Name);
                        break;
                }
                int pageSize = 8;
                return View(await PaginatedList<Course>.CreateAsync((IQueryable<Course>)courses, pageNumber ?? 1, pageSize));
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Optionally, log additional details
                // Log the SQL statement causing the exception.
                Console.WriteLine($"SQL: {ex.InnerException?.InnerException?.Message}");
                ModelState.AddModelError("", "An error occurred while retrieving data from the database.");

                ViewBag.Message = "An error occurred while retrieving data from the database.";
                return View();
            }
        }


    }
}
