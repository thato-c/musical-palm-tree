using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using OnlineCampus.Repositories;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly ILogger<StudentController> _logger;
        private IStudentRepository _studentRepository;
        private IAuthRepository _authRepository;

        public StudentController(ILogger<StudentController> logger, IStudentRepository studentRepository, IAuthRepository authRepository)
        {
            _logger = logger;
            _studentRepository = studentRepository;
            _authRepository = authRepository;

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
                        students.OrderBy(s => s.LastName);
                        break;
                }
                int pageSize = 8;
                return View(await PaginatedList<Student>.CreateAsync((IQueryable<Student>)students, pageNumber ?? 1, pageSize));
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
                        var Student = new Models.Student
                        {
                            FirstName = viewModel.FirstName,
                            LastName = viewModel.LastName,
                            UserId = user.Id,
                        };

                        // Add and save the new student to the database
                        _studentRepository.InsertStudent(Student);
                        _studentRepository.Save();
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
                ViewBag.Message = "An error occurred while inserting data into the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid StudentId)
        {
            try
            {
                var student = await _studentRepository.GetStudentByIdAsync(StudentId);

                if (student == null)
                {
                    ViewBag.Message = "The Student has not been found.";
                    return View();
                }

                var viewModel = new StudentDetailViewModel
                {
                    StudentId = student.StudentId,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    RowVersion = student.RowVersion
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
                ViewBag.Message = "An error occurred while retrieving data from the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid StudentId)
        {
            var student = await _studentRepository.GetStudentWithCoursesByIdAsync(StudentId);
            if (student == null)
            {
                return NotFound();
            }

            var courses = student.Enrolments.Select(s => s.Course).ToList();

            var viewModel = new StudentDetailsViewModel
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
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
                            _studentRepository.SetOriginalRowVersion(studentToEdit, viewModel.RowVersion);

                            studentToEdit.FirstName = viewModel.FirstName;
                            studentToEdit.LastName = viewModel.LastName;

                            try
                            {
                                _studentRepository.UpdateStudent(studentToEdit);
                                await _studentRepository.SaveAsync();
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
                        ViewBag.Message = "Student was not found.";
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
                ViewBag.Message = "An error occurred while editing data in the database.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid StudentId)
        {
            try
            {
                var student = await _studentRepository.GetStudentByIdAsync(StudentId);

                if (student == null)
                {
                    ViewBag.Message = "The Student has not been found.";
                    return View();
                }

                var viewModel = new StudentDetailViewModel
                {
                    StudentId = student.StudentId,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    RowVersion = student.RowVersion
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
                ViewBag.Message = "An error occurred while retrieving data from the database.";
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
                    var studentToDelete = await _studentRepository.GetStudentByIdAsync(viewModel.StudentId);

                    if (studentToDelete == null)
                    {
                        ViewBag.Message = "Student was not found.";
                        return View();
                    }

                    try
                    {
                        await _studentRepository.DeleteStudent(viewModel.StudentId);
                        await _studentRepository.SaveAsync();

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
                ViewBag.Message = "An error occurred while removing data from the database.";
                return View();
            }
        }
    }
}
