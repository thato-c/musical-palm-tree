using OnlineCampus.DTOs;
using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface ICourseService
    {
        Task<OperationResultWithData<Course>> GetCourseByIdAsync(Guid courseId);
        Task<OperationResultWithData<Course>> GetCourseWithEnrolledStudentsAsync(Guid courseId);
        Task<OperationResult> CreateCourseAsync(Course course);
        Task<OperationResult> UpdateCourseAsync(Course course);
        Task<OperationResult> DeleteCourseAsync(Guid courseId);
    }
}
