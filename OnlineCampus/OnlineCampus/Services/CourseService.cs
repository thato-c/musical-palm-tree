using OnlineCampus.DTOs;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<OperationResult> CreateCourseAsync(Course course)
        {
            _courseRepository.InsertCourse(course);
            await _courseRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResult> UpdateCourseAsync(Course course)
        {
            _courseRepository.SetOriginalRowVersion(course, course.RowVersion);
            _courseRepository.UpdateCourse(course);
            await _courseRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResult> DeleteCourseAsync(Guid courseId)
        {
            await _courseRepository.DeleteCourse(courseId);
            await _courseRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResultWithData<Course>> GetCourseByIdAsync(Guid courseId)
        {
            var course = await _courseRepository.GetCourseByIdAsync(courseId);

            if (course == null)
            {
                return new OperationResultWithData<Course> { Success = false, Message = "Course not found" };
            }

            return new OperationResultWithData<Course> { Success = true, Data = course };
        }

        public async Task<OperationResultWithData<Course>> GetCourseWithEnrolledStudentsAsync(Guid courseId)
        {
            var course = await _courseRepository.GetCourseWithStudentsByIdAsync(courseId);

            if (course == null)
            {
                return new OperationResultWithData<Course> { Success = false, Message = "Course not found" };
            }

            return new OperationResultWithData<Course> { Success = true, Data = course };
        }
    }
}
