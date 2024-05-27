using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Models;
using OnlineCampus.ViewModels;

namespace OnlineCampus.Controllers
{
    public class StudentController : Controller
    {
        private readonly EnrolmentDBContext _context;

        public StudentController(EnrolmentDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchString,string currentFilter, int? pageNumber)
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
                var students = from s in _context.Students select s;

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
                int pageSize = 3;
                return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
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
                    // Map the ViewModel to the Student Model
                    var Student = new Models.Student
                    {
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                    };

                    // Add and save the new student to the database
                    _context.Students.Add(Student);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                return View(viewModel);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Optionally, log additional details
                // Log the SQL statement causing the exception
                Console.WriteLine($"SQL: {ex.InnerException?.InnerException?.Message}");
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
                var student = await _context.Students
                .Where(s => s.StudentId == StudentId)
                .Select(s => new StudentDetailViewModel
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    RowVersion = s.RowVersion,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

                if (student != null)
                {
                    return View(student);
                }
                else
                {
                    ViewBag.Message = "The Student has not been found.";
                    return View();
                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var studentToEdit = await _context.Students
                        .Where(s => s.StudentId == viewModel.StudentId)
                        .FirstOrDefaultAsync();

                    if (studentToEdit != null)
                    {
                        if (studentToEdit.FirstName == viewModel.FirstName && studentToEdit.LastName == viewModel.LastName)
                        {
                            ModelState.AddModelError(string.Empty, "Data has not been modified");
                            return View(viewModel);
                        }
                        else
                        {
                            _context.Entry(studentToEdit).Property("RowVersion").OriginalValue = viewModel.RowVersion;
                            studentToEdit.FirstName = viewModel.FirstName;
                            studentToEdit.LastName = viewModel.LastName;

                            try
                            {
                                _context.Update(studentToEdit);
                                await _context.SaveChangesAsync();

                                return RedirectToAction("Index");
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var exceptionEntry = ex.Entries.Single();
                                var databaseEntry = exceptionEntry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    ModelState.AddModelError(string.Empty, "Unable to save changes. The student was deleted by another user.");
                                }
                                else
                                {
                                    var databaseValues = (Student)databaseEntry.ToObject();

                                    if (databaseValues.FirstName != studentToEdit.FirstName)
                                    {
                                        ModelState.AddModelError("FirstName", $"Current Value: {databaseValues.FirstName}");
                                    }
                                    if (databaseValues.LastName != studentToEdit.LastName)
                                    {
                                        ModelState.AddModelError("LastName", $"Current Value: {databaseValues.LastName}");
                                    }

                                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                        + "was modified by another user after you got the original value. The "
                                        + "edit operation was canceled and the current values in the database "
                                        + "have been displayed. If you still want to edit this record, click "
                                        + "the Save button again. Otherwise click the Back to List hyperlink.");

                                    //TODO: Update the viewModel values to the newly read database values
                                    viewModel.StudentId = databaseValues.StudentId;
                                    viewModel.FirstName = databaseValues.FirstName; 
                                    viewModel.LastName = databaseValues.LastName;
                                    viewModel.RowVersion = databaseValues.RowVersion;
                                }
                                return View(viewModel);
                            }
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
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Optionally, log additional details
                // Log the SQL statement causing the exception
                Console.WriteLine($"SQL: {ex.InnerException?.InnerException?.Message}");
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
                var student = await _context.Students
                .Where(s => s.StudentId == StudentId)
                .Select(s => new StudentDetailViewModel
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

                if (student != null)
                {
                    return View(student);
                }
                else
                {
                    ViewBag.Message = "The Student has not been found.";
                    return View();
                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(StudentDetailViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var studentToDelete = await _context.Students
                        .Where(s => s.StudentId == viewModel.StudentId)
                        .FirstOrDefaultAsync();

                    if (studentToDelete != null)
                    {
                        _context.Remove(studentToDelete);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index");
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
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Optionally, log additional details
                // Log the SQL statement causing the exception
                Console.WriteLine($"SQL: {ex.InnerException?.InnerException?.Message}");
                ModelState.AddModelError("", "An error occurred while removing data from the database.");

                ViewBag.Message = "An error occurred while removing data from the database.";
                return View();
            }
        }
    }
}
