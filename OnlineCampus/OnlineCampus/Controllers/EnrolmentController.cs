using Microsoft.AspNetCore.Mvc;
using OnlineCampus.Interfaces;
using System.Security.Claims;

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
            var course = await _courseRepository.GetCourseByIdAsync(CourseId);
            if (course == null)
            {
                ViewBag.Message = "The course has not been found";
                return View("Course", "Index");
            }

            var courseId = course.CourseId;

            // Create a User repository
            var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (studentIdString == null)
            {
                // Redirect to working Login page
                return View("Login");
            }

            if (!Guid.TryParse(studentIdString, out Guid studentId))
            {
                return BadRequest("Invalid User ID");
            }

            // Correct naming issues.
            var enrollment = new Models.Enrolment
            {
                CourseId = courseId,
                StudentId = studentId,
            };

            _enrolmentRepository.InsertEnrolment(enrollment);
            _enrolmentRepository.Save();

            // Correct the redirection, causes an error.
            return View("Index", "Course");
        }
    }


}
