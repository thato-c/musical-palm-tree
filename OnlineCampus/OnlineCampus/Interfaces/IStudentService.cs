using OnlineCampus.DTOs;
using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IStudentService
    {
        Task<OperationResultWithData<List<Student>>> GetAllStudentsAsync();
        Task<OperationResultWithData<Student>> GetStudentByIdAsync(Guid studentId);
        Task<OperationResultWithData<Student>> GetStudentByIdWithEnrolledCoursesAsync(Guid studentId);
        Task<OperationResult> CreateStudentAsync(Student student);
        Task<OperationResult> UpdateStudentAsync(Student student);
        Task<OperationResult> DeleteStudentAsync(Guid studentId);
    }
}
