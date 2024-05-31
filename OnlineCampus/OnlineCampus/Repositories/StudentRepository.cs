using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Repositories
{
    public class StudentRepository : IStudentRepository, IDisposable
    {
        private EnrolmentDBContext context;

        public StudentRepository(EnrolmentDBContext context)
        {
            this.context = context;
        }

        public IQueryable<Student> GetStudents()
        {
            return context.Students.AsQueryable();
        }
        
        public async Task<Student> GetStudentByIdAsync(Guid studentId)
        {
            return await context.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public void InsertStudent(Student student)
        {
            context.Students.Add(student);
        }

        public async Task<Student> DeleteStudent(Guid studentId)
        {
            var student = await context.Students.FindAsync(studentId);

            if (student != null)
            {
                context.Students.Remove(student);
            }

            return null;
        }

        public void UpdateStudent(Student student)
        {
            context.Entry(student).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
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
