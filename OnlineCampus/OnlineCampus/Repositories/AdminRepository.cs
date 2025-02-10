using Microsoft.EntityFrameworkCore;
using OnlineCampus.Data;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Repositories
{
    public class AdminRepository : IAdminRepository, IDisposable
    {
        private EnrolmentDBContext context;

        public AdminRepository(EnrolmentDBContext context)
        {
            this.context = context;
        }

        public IQueryable<Admin> GetAdmins()
        {
            return context.Admins.AsQueryable();
        }

        public async Task<Admin> GetAdminByIdAsync(Guid adminId)
        {
            return await context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.AdminId == adminId);
        }

        public async Task<Guid?> GetAdminIdAsync(Guid adminId)
        {
            return await context.Admins
                .Where(c => c.AdminId == adminId)
                .Select(c => c.AdminId)
                .FirstOrDefaultAsync();
        }

        public void InsertAdmin(Admin admin)
        {
            context.Admins.Add(admin);
        }

        public async Task<Admin> DeleteAdmin(Guid adminId)
        {
            var admin = await context.Admins.FindAsync(adminId);

            if (admin != null)
            {
                context.Admins.Remove(admin);
            }

            return null;
        }

        public void UpdateAdmin(Admin admin)
        {
            context.Entry(admin).State = EntityState.Modified;
        }

        public void SetOriginalRowVersion(Admin admin, byte[] rowVersion)
        {
            context.Entry(admin).Property("RowVersion").OriginalValue = rowVersion;
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
