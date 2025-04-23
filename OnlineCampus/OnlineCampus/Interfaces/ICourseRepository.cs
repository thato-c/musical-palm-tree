using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface ICourseRepository:IDisposable
    {
        IQueryable<Course> GetCourses();
        Task<Course> GetCourseByIdAsync(Guid courseId);
        Task<Course> GetCourseWithStudentsByIdAsync(Guid courseId);
        Task<Guid?> GetCourseIdAsync(Guid courseId);
        void InsertCourse(Course course);
        Task<Course> DeleteCourse(Guid courseId);
        void UpdateCourse(Course course);
        void SetOriginalRowVersion(Course course, byte[] rowVersion);
        void Save();
        Task SaveAsync();
    }
}
