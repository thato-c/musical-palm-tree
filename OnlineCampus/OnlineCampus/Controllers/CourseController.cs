using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.ViewModels;

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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Map the ViewModel to the Course MModel
                    var course = new Models.Course
                    {
                        Code = viewModel.Code,
                        Name = viewModel.Name,
                        Description = viewModel.Description,
                        Credits = viewModel.Credits,
                    };

                    // Add and save the nre course to the database
                    courseRepository.InsertCourse(course);
                    courseRepository.Save();
                    return RedirectToAction("Index");
                }
                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Log thee exception details.
                _logger.LogError(ex, "AN error occurred while inserting data into the database.");

                // Optionally, log additional details.
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                if (ex.InnerException?.InnerException != null)
                {
                    _logger.LogError("SQL: {Message}", ex.InnerException.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while inserting data into the database.");
                ViewBag.Message = "An error occurred while inserting data into the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid CourseId)
        {
            try
            {
                var course = await courseRepository.GetCourseByIdAsync(CourseId);

                if (course == null)
                {
                    ViewBag.Message = "The Course has not been found.";
                    return View();
                }

                var viewModel = new CourseDetailViewModel
                {
                    CourseId = course.CourseId,
                    Code = course.Code,
                    Name = course.Name,
                    Description = course.Description,
                    Credits = course.Credits,
                    RowVersion = course.RowVersion,
                };

                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while retrieving data from the database.");

                // Optionally, log additional details
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                if (ex.InnerException?.InnerException != null)
                {
                    _logger.LogError("SQL: {Message}", ex.InnerException?.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while retrieving data from the database.");
                ViewBag.Message = "An error occurred while retrieving data from the database.";
                return View();
            }
        }

    }
}
