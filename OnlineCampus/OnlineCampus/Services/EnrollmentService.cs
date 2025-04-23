using OnlineCampus.DTOs;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using System.Security.Claims;

namespace OnlineCampus.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrolmentRepository _enrolmentRepository;

        public EnrollmentService(ICourseRepository courseRepository,
                                 IStudentRepository studentRepository,
                                 IEnrolmentRepository enrolmentRepository)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _enrolmentRepository = enrolmentRepository;
        }

        public async Task<EnrollmentResult> EnrollStudentAsync(Guid courseId, Guid userId)
        {
            var courseExists = await _courseRepository.GetCourseByIdAsync(courseId);
            if (courseExists == null)
            {
                return new EnrollmentResult { Success = false, ErrorMessage = "Course not found" };
            }

            var studentId = await _studentRepository.GetStudentIdAsync(userId);
            if (studentId == null)
            {
                return new EnrollmentResult { Success = false, ErrorMessage = "Student not registered" };
            }

            var enrolment = new Enrolment
            {
                CourseId = courseId,
                StudentId = studentId.Value
            };

            _enrolmentRepository.InsertEnrolment(enrolment);
            _enrolmentRepository.Save();

            return new EnrollmentResult { Success = true };
        }

    }
}
