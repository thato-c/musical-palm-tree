using Microsoft.AspNetCore.Mvc;
using OnlineCampus.Interfaces;
using System.Security.Claims;
using OnlineCampus.ViewModels;


namespace OnlineCampus.Controllers
{
    public class EnrolmentController:Controller
    {
        private readonly ILogger<EnrolmentController> _logger;
        private IEnrolmentRepository _enrolmentRepository;
        private IStudentRepository _studentRepository;
        private ICourseRepository _courseRepository;

        public EnrolmentController(ILogger<EnrolmentController> logger, 
                                    IEnrolmentRepository enrolmentRepository, 
                                    IStudentRepository studentRepository,
                                    ICourseRepository courseRepository)
        {
            _logger = logger;
            _enrolmentRepository = enrolmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
        }

        
        [HttpGet]
        public async Task<IActionResult> EnrollStudent(Guid CourseId)
        {
            var courseId = await _courseRepository.GetCourseIdAsync(CourseId);
            if (courseId == null)
            {
                ViewBag.Message = "The course has not been found";
                return View("Course", "Index");
            }

            if (User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    return BadRequest("Invalid User Id");
                }

                var studentIdString = await _studentRepository.GetStudentIdAsync(userId);

                if (studentIdString == null)
                {
                    // Redirect to working Login page
                    return LocalRedirect("~/Identity/Account/Register");
                }

                Guid studentId = studentIdString.Value;

                // Correct naming issues.
                var enrollment = new Models.Enrolment
                {
                    CourseId = (Guid)courseId.Value,
                    StudentId = studentId,
                };

                _enrolmentRepository.InsertEnrolment(enrollment);

                _enrolmentRepository.Save();

                // Correct the redirection, causes an error.
                return RedirectToAction("Index", "Course");
            }


            return LocalRedirect("~/Identity/Account/Register");

            
        }
    }


}
