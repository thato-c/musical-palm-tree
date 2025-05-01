using OnlineCampus.DTOs;
using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface ICourseService
    {
        Task<OperationResult> CreateCourseAsync(Course course);
        Task<OperationResult> UpdateCourseAsync(Course course);
        Task<OperationResult> DeleteCourseAsync(Guid courseId);
    }
}
