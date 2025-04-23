using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using SQLitePCL;

namespace OnlineCampus.Repositories
{
    public class CourseRepository : ICourseRepository, IDisposable
    {
        private EnrolmentDBContext context;

        public CourseRepository(EnrolmentDBContext context)
        {
            this.context = context;
        }

        public IQueryable<Course> GetCourses()
        {
            return context.Courses.AsQueryable();
        }

        public async Task<Course> GetCourseWithStudentsByIdAsync(Guid courseId)
        {
            return await context.Courses.Where(c => c.CourseId == courseId)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync();
        }

        public async Task<Course> GetCourseByIdAsync(Guid courseId)
        {
            return await context.Courses
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Guid?> GetCourseIdAsync(Guid courseId)
        {
            return await context.Courses
                .Where(c => c.CourseId == courseId)
                .Select(c => c.CourseId)
                .FirstOrDefaultAsync();
        }

        public void InsertCourse(Course course)
        {
            context.Courses.Add(course);
        }

        public async Task<Course> DeleteCourse(Guid courseId)
        {
            var course = await context.Courses.FindAsync(courseId);

            if (course != null)
            {
                context.Courses.Remove(course);
            }

            return null;
        }

        public void UpdateCourse(Course course)
        {
            context.Entry(course).State = EntityState.Modified;
        }

        public void SetOriginalRowVersion(Course course, byte[] rowVersion)
        {
            context.Entry(course).Property("RowVersion").OriginalValue = rowVersion;
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
