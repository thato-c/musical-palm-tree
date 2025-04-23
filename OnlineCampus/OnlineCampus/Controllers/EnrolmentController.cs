using Microsoft.AspNetCore.Mvc;
using OnlineCampus.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace OnlineCampus.Controllers
{
    [AllowAnonymous]
    public class EnrolmentController:Controller
    {
        private readonly ILogger<EnrolmentController> _logger;
        private IEnrollmentService _enrollmentService;
        private IAuthRepository _authRepository;

        public EnrolmentController(ILogger<EnrolmentController> logger, 
                                    IEnrollmentService enrollmentService,
                                    IAuthRepository authRepository)
        {
            _logger = logger;
            _enrollmentService = enrollmentService;
            _authRepository = authRepository;
        }

        
        [HttpGet]
        public async Task<IActionResult> EnrollStudent(Guid CourseId)
        {
            // Ensure the User is Authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userId = _authRepository.GetUserId(User);
                if (userId == null)
                {
                    return BadRequest("Invalid user Id");
                }

                var result = await _enrollmentService.EnrollStudentAsync(CourseId, userId.Value);

                if (!result.Success)
                {
                    ViewBag.Message = "We couldn't complete your request. Please try again or contact support.";
                    return RedirectToAction("Index", "Course");
                }
                return RedirectToAction("Index", "Course");
            }
            return LocalRedirect("~/Identity/Account/Register");            
        }
    }
}
