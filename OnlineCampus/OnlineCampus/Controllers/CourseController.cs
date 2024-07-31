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

        [HttpPost]
        public async Task<IActionResult> Edit(CourseDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var courseToEdit = await courseRepository.GetCourseByIdAsync(viewModel.CourseId);

                    if (courseToEdit != null)
                    {
                        if (courseToEdit.Code == viewModel.Code && 
                            courseToEdit.Name == viewModel.Name &&
                            courseToEdit.Credits == viewModel.Credits &&
                            courseToEdit.Description == viewModel.Description)
                        {
                            ModelState.AddModelError(string.Empty, "Data has not been modified.");
                            return View(viewModel);
                        }
                        else
                        {
                            courseRepository.SetOriginalRowVersion(courseToEdit, viewModel.RowVersion);

                            if (await TryUpdateModelAsync<Course>(
                                courseToEdit,
                                "",
                                c => c.Code, c => c.Name, c => c.Credits, c => c.Description))
                            {
                                try
                                {
                                    courseRepository.UpdateCourse(courseToEdit);
                                    courseRepository.Save();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var exceptionEntry = ex.Entries.Single();
                                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        ModelState.AddModelError(string.Empty, "Unable to save changes. The course was deleted by another user.");
                                        return View(viewModel);
                                    }
                                    else
                                    {
                                        var databaseValues = (Course)databaseEntry.ToObject();
                                        var updatedCourse = new CourseDetailViewModel
                                        {
                                            CourseId = databaseValues.CourseId,
                                            Name = databaseValues.Name,
                                            Credits = databaseValues.Credits,
                                            Description = databaseValues.Description,
                                            RowVersion = databaseValues.RowVersion,
                                        };

                                        if (databaseValues.Code != courseToEdit.Code)
                                        {
                                            ModelState.AddModelError("Code", $"Current Value: {databaseValues.Code}");
                                        }
                                        if (databaseValues.Name != courseToEdit.Name)
                                        {
                                            ModelState.AddModelError("Name", $"Current Value: {databaseValues.Name}");
                                        }
                                        if (databaseValues.Credits != courseToEdit.Credits)
                                        {
                                            ModelState.AddModelError("Credits", $"Current Value: {databaseValues.Credits}");
                                        }
                                        if (databaseValues.Description != courseToEdit.Description)
                                        {
                                            ModelState.AddModelError("Description", $"Current Value: {databaseValues.Description}");
                                        }

                                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                            + "was modified by another user after you got the original value. " 
                                            + "The edit operation was cancelled and the current values in the database" 
                                            + "have been dispplayed. If you still want to edit this record, click " 
                                            + "the Save button again. Otherwise click the Back to List hyperlink.");

                                        return View(updatedCourse);
                                    }
                                }
                            }

                            return RedirectToAction("Index");
                        }
                    }

                    ViewBag.Message = "Course was not found.";
                    return View();

                }

                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while editing data in the database.");

                // Optionally, log additional details
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                if (ex.InnerException?.InnerException != null)
                {
                    _logger.LogError("SQL: {Message}", ex.InnerException.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while editing data in the database.");
                ViewBag.Message = "An error occurred while editing data in the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid CourseId)
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
                    Name = course.Name,
                    Description = course.Description,
                    Code = course.Code,
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(CourseDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var courseToDelete = await courseRepository.GetCourseByIdAsync(viewModel.CourseId);

                    if (courseToDelete == null)
                    {
                        ViewBag.Message = "Course was not found.";
                        return View();
                    }

                    try
                    {
                        await courseRepository.DeleteCourse(viewModel.CourseId);
                        await courseRepository.SaveAsync();
                        return RedirectToAction("Index");
                    }
                    catch (DbUpdateException ex)
                    {
                        var ExceptionEntry = ex.Entries.Single();
                        var databaseEntry = ExceptionEntry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError(string.Empty, "Unable to save the changes. The course has been deleted by aniother user.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Concurrency error occurred.");
                        }
                        return View(viewModel);
                    }
                }
                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while removing data from the database.");

                // Optionally, log additional details
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                if (ex.InnerException?.InnerException != null)
                {
                    _logger.LogError("SQL: {Message}", ex.InnerException?.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while removing data from the database.");
                ViewBag.Message = "An error occurred while removing data from the database.";
                return View();
            }
        }
    }
}
