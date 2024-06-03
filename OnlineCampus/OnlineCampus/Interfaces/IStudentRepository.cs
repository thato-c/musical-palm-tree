using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IStudentRepository:IDisposable
    {
        IQueryable<Student> GetStudents();
        Task<Student> GetStudentByIdAsync(Guid studentId);
        void InsertStudent(Student student);
        Task<Student> DeleteStudent(Guid studentId);
        void UpdateStudent(Student student);
        void SetOriginalRowVersion(Student student, byte[] rowVersion);
        void Save();
        Task SaveAsync();
    }
}
