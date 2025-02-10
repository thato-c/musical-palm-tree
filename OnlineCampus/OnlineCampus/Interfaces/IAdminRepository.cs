using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IAdminRepository:IDisposable
    {
        IQueryable<Admin> GetAdmins();
        Task<Admin> GetAdminByIdAsync(Guid adminId);
        Task<Guid?> GetAdminIdAsync(Guid adminId);
        void InsertAdmin(Admin admin);
        Task<Admin> DeleteAdmin(Guid adminId);
        void UpdateAdmin(Admin admin);
        void SetOriginalRowVersion(Admin admin, byte[] rowVersion);
        void Save();
        Task SaveAsync();
    }
}
