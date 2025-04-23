using OnlineCampus.DTOs;
using System.Security.Claims;

namespace OnlineCampus.Interfaces
{
    public interface IEnrollmentService
    {
        Task<EnrollmentResult> EnrollStudentAsync(Guid courseId, Guid userId);
    }
}
