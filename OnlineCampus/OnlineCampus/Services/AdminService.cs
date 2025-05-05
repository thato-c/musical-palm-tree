using Microsoft.EntityFrameworkCore;
using OnlineCampus.DTOs;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;

namespace OnlineCampus.Services
{
    public class AdminService: IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<OperationResultWithData<List<Admin>>> GetAllAdminsAsync()
        {
            var admins = await _adminRepository.GetAdmins().ToListAsync();

            if (admins == null)
            {
                return new OperationResultWithData<List<Admin>> { Success = false, Message = "No admins were found" };
            }
            return new OperationResultWithData<List<Admin>> { Success = true, Data = admins };
        }

        public async Task<OperationResultWithData<Admin>> GetAdminByIdAsync(Guid AdminId)
        {
            var admin = await _adminRepository.GetAdminByIdAsync(AdminId);

            if (admin == null)
            {
                return new OperationResultWithData<Admin> { Success = false, Message = "Admin not found" };
            }
            return new OperationResultWithData<Admin> {Success = true, Data = admin };
        }

        public async Task<OperationResult> CreateAdminAsync(Admin admin)
        {
            _adminRepository.InsertAdmin(admin);
            await _adminRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResult> UpdateAdminAsync(Admin admin)
        {
            _adminRepository.SetOriginalRowVersion(admin, admin.RowVersion);
            _adminRepository.UpdateAdmin(admin);
            await _adminRepository.SaveAsync();
            return new OperationResult { Success = true };
        }

        public async Task<OperationResult> DeleteAdminAsync(Guid adminId)
        {
            await _adminRepository.DeleteAdmin(adminId);
            await _adminRepository.SaveAsync();
            return new OperationResult { Success = true };
        }
    }
}
