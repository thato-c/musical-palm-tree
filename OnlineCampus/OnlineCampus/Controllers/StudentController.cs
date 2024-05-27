using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
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
        public async Task<IActionResult> Index()
        {
            try
            {
                var students = await _context.Students.AsNoTracking().ToListAsync();

                if (students.Count == 0)
                {
                    ViewBag.Message = "No Students Exist";
                    return View();
                }
                else
                {
                    return View(students);
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
                                Console.WriteLine("Something's wrong.");
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
