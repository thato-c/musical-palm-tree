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
        private readonly IEnrollmentRepository _enrolmentRepository;

        public EnrollmentService(ICourseRepository courseRepository,
                                 IStudentRepository studentRepository,
                                 IEnrollmentRepository enrolmentRepository)
        {
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _enrolmentRepository = enrolmentRepository;
        }

        public async Task<OperationResult> EnrollStudentAsync(Guid courseId, Guid userId)
        {
            var courseExists = await _courseRepository.GetCourseByIdAsync(courseId);
            if (courseExists == null)
            {
                return new OperationResult { Success = false, Message = "Course not found" };
            }

            var studentId = await _studentRepository.GetStudentIdAsync(userId);
            if (studentId == null)
            {
                return new OperationResult { Success = false, Message = "Student not registered" };
            }

            var enrollment = new Enrolment
            {
                CourseId = courseId,
                StudentId = studentId.Value
            };

            _enrolmentRepository.InsertEnrollment(enrollment);
            _enrolmentRepository.Save();

            return new OperationResult { Success = true };
        }

    }
}
