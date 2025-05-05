using OnlineCampus.DTOs;
using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IAdminService
    {
        Task<OperationResultWithData<List<Admin>>> GetAllAdminsAsync();
        Task<OperationResultWithData<Admin>> GetAdminByIdAsync(Guid AdminId);
        Task<OperationResult> CreateAdminAsync(Admin admin);
        Task<OperationResult> UpdateAdminAsync(Admin admin);
        Task<OperationResult> DeleteAdminAsync(Guid AdminId);
    }
}
