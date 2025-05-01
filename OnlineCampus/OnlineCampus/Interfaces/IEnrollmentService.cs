using OnlineCampus.DTOs;
using System.Security.Claims;

namespace OnlineCampus.Interfaces
{
    public interface IEnrollmentService
    {
        Task<OperationResult> EnrollStudentAsync(Guid courseId, Guid userId);
    }
}
