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
    }
}
