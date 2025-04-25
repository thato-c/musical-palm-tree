using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Repositories
{
    public class StudentRepository : IStudentRepository, IDisposable
    {
        private EnrolmentDBContext _context;

        public StudentRepository(EnrolmentDBContext context)
        {
            _context = context;
        }

        public IQueryable<Student> GetStudents()
        {
            return _context.Students.AsQueryable();
        }
   
        public async Task<Student> GetStudentWithCoursesByIdAsync(Guid studentId)
        {
            return await _context.Students.Where(s => s.StudentId == studentId)
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync();
        }

        public async Task<Guid?> GetStudentIdAsync(Guid userId)
        {
            return await _context.Students
                .Where(s => s.UserId == userId.ToString())
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();
        }

        public async Task<Student> GetStudentByIdAsync(Guid studentId)
        {
            return await _context.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public void InsertStudent(Student student)
        {
            _context.Students.Add(student);
        }

        public async Task<Student> DeleteStudent(Guid studentId)
        {
            var student = await _context.Students.FindAsync(studentId);

            if (student != null)
            {
                _context.Students.Remove(student);
            }

            return null;
        }

        public void UpdateStudent(Student student)
        {
            _context.Entry(student).State = EntityState.Modified;
        }

        public void SetOriginalRowVersion(Student student, byte[] rowVersion)
        {
            _context.Entry(student).Property("RowVersion").OriginalValue = rowVersion;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
