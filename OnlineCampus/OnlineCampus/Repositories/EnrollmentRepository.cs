using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Repositories
{
    public class EnrollmentRepository:IEnrollmentRepository, IDisposable
    {
        private EnrolmentDBContext _context;

        public EnrollmentRepository(EnrolmentDBContext context)
        {
            _context = context;
        }

        public IQueryable<Enrolment> GetEnrollments()
        {
            return _context.Enrolments.AsQueryable();
        }

        public async Task<Enrolment> GetEnrollmentByIdAsync(Guid enrolmentId)
        {
            return await _context.Enrolments.AsNoTracking().
                FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
        }

        public void InsertEnrollment(Enrolment enrolment)
        {
            _context.Enrolments.Add(enrolment);
        }

        public async Task<Enrolment> DeleteEnrollment(Guid enrolmentId)
        {
            var enrolment = await _context.Enrolments.FirstAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment != null)
            {
                _context.Enrolments.Remove(enrolment);
            }

            return null;
        }

        public void UpdateEnrollment(Enrolment enrolment)
        {
            _context.Entry(enrolment).State = EntityState.Modified;
        }
        
        public void SetOriginalRowVersion(Enrolment enrolment, byte[] rowVersion)
        {
            _context.Entry(enrolment).Property("RowVersion").OriginalValue = rowVersion;
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
