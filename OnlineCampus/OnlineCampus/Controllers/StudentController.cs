using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineCampus.Data;
using OnlineCampus.DTOs;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.Repositories;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Controllers
{
    [Authorize(Roles = "Admin, SuperAdmin")]
    public class StudentController : Controller
    {
        private readonly ILogger<StudentController> _logger;
        private IStudentRepository _studentRepository;
        private IStudentService _studentService;
        private IAuthRepository _authRepository;
        private readonly int _pageSize;

        public StudentController(ILogger<StudentController> logger, IStudentRepository studentRepository, IAuthRepository authRepository, IStudentService studentService, IOptions<PaginationSettings> paginationSettings)
        {
            _logger = logger;
            _studentRepository = studentRepository;
            _authRepository = authRepository;
            _pageSize = paginationSettings.Value.PageSize;
            _studentService = studentService;
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
                var students = from s in _studentRepository.GetStudents() 
                               select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    students = students.Where(s => s.LastName.Contains(searchString)
                                                || s.FirstName.Contains(searchString));
                }
                switch (sortOrder)
                {
                    case "name_desc":
                        students = students.OrderByDescending(s => s.LastName);
                        break;
                    default:
                        students = students.OrderBy(s => s.LastName);
                        break;
                }
                return View(await PaginatedList<Student>.CreateAsync((IQueryable<Student>)students, pageNumber ?? 1, _pageSize));
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Optionally, log additional details
                // Log the SQL statement causing the exception
                Console.WriteLine($"SQL: {ex.InnerException?.InnerException?.Message}");
                ModelState.AddModelError("", "An error occurred while retrieving data from the database.");

                TempData["Message"] = "An error occurred while retrieving data from the database.";
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
        public async Task<IActionResult> Create(StudentViewModel viewModel)
        {
            try
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
                        await _authRepository.AssignRoleAsync(user, "Student");

                        // Map the ViewModel to the Student Model
                        var student = new Models.Student
                        {
                            FirstName = viewModel.FirstName,
                            LastName = viewModel.LastName,
                            UserId = user.Id,
                        };

                        // Add and save the new student to the database
                        var operationResult = await _studentService.CreateStudentAsync(student);
                        if (!operationResult.Success)
                        {
                            TempData["Message"] = "We couldn't complete your request. Please try again or contact support.";
                            return RedirectToAction("Index");
                        }
                        await _authRepository.SendConfirmationEmailAsync(user, Url.Content("~/"));
                        return RedirectToAction("Index");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(viewModel);
                }
                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred while inserting data into the database.");

                // Optionally, log additional details
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                if (ex.InnerException?.InnerException != null)
                {
                    _logger.LogError("SQL: {Message}", ex.InnerException.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while inserting data into the database.");
                TempData["Message"] = "An error occurred while inserting data into the database.";
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid StudentId)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(StudentId);

                if (student.Data == null)
                {
                    TempData["Message"] = "The Student has not been found.";
                    return View();
                }

                var viewModel = new StudentDetailViewModel
                {
                    StudentId = student.Data.StudentId,
                    FirstName = student.Data.FirstName,
                    LastName = student.Data.LastName,
                    RowVersion = student.Data.RowVersion
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
                    _logger.LogError("SQL: {Message}", ex.InnerException.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while retrieving data from the database.");
                TempData["Message"] = "An error occurred while retrieving data from the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid StudentId)
        {
            var student = await _studentService.GetStudentByIdWithEnrolledCoursesAsync(StudentId);
            if (student.Data == null)
            {
                return NotFound();
            }

            var courses = student.Data.Enrolments.Select(s => s.Course).ToList();

            var viewModel = new StudentDetailsViewModel
            {
                StudentId = student.Data.StudentId,
                FirstName = student.Data.FirstName,
                LastName = student.Data.LastName,
                EnrolledCourses = courses,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var studentToEdit = await _studentRepository.GetStudentByIdAsync(viewModel.StudentId);

                    if (studentToEdit != null)
                    {
                        
                        if (studentToEdit.FirstName == viewModel.FirstName && studentToEdit.LastName == viewModel.LastName)
                        {
                            ModelState.AddModelError(string.Empty, "Data has not been modified");
                            return View(viewModel);
                        }
                        else
                        {
                            studentToEdit.FirstName = viewModel.FirstName;
                            studentToEdit.LastName = viewModel.LastName;

                            try
                            {
                                var result = await _studentService.UpdateStudentAsync(studentToEdit);
                                if (!result.Success)
                                {
                                    TempData["Message"] = "We couldn't complete your request. Please try again or contwct support.";
                                    return RedirectToAction("Index");
                                }
                                return RedirectToAction("Index");
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                _logger.LogError(ex, "Concurrency error while updating student");
                                ModelState.AddModelError("", "Concurrency error. Please try again.");
                            }

                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        TempData["Message"] = "Student was not found.";
                        return View();
                    }
                }
                else
                {
                    return View(viewModel);
                }
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
                TempData["Message"] = "An error occurred while editing data in the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid StudentId)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(StudentId);

                if (student.Data == null)
                {
                    TempData["Message"] = "The Student has not been found.";
                    return View();
                }

                var viewModel = new StudentDetailViewModel
                {
                    StudentId = student.Data.StudentId,
                    FirstName = student.Data.FirstName,
                    LastName = student.Data.LastName,
                    RowVersion = student.Data.RowVersion
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
                    _logger.LogError("SQL: {Message}", ex.InnerException.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while retrieving data from the database.");
                TempData["Message"] = "An error occurred while retrieving data from the database.";
                return View();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(StudentDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var studentToDelete = await _studentService.GetStudentByIdAsync(viewModel.StudentId);

                    if (studentToDelete.Data == null)
                    {
                        TempData["Message"] = "Student was not found.";
                        return View();
                    }

                    try
                    {
                        var result = await _studentService.DeleteStudentAsync(viewModel.StudentId);
                        if (!result.Success)
                        {
                            TempData["Message"] = "We couldn't complete your request. Please try again or contact support.";
                            return RedirectToAction("Index");
                        }
                        return RedirectToAction("Index");
                    }
                    catch(DbUpdateConcurrencyException ex)
                    {
                        var ExceptionEntry = ex.Entries.Single();
                        var databaseEntry = ExceptionEntry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError(string.Empty, "Unable to save the changes. The student has been deleted by another user.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Concurrency error occurred.");
                        }
                        return View(viewModel);
                    }
                }
                else
                {
                    return View(viewModel);
                }
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
                    _logger.LogError("SQL: {Message}", ex.InnerException.InnerException.Message);
                }

                ModelState.AddModelError("", "An error occurred while removing data from the database.");
                TempData["Message"] = "An error occurred while removing data from the database.";
                return View();
            }
        }
    }
}
