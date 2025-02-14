using Microsoft.AspNetCore.Identity;

namespace OnlineCampus.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<IdentityRole>> GetAllRolesAsync();
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> DeleteRoleAsync(string roleId);
        Task<IdentityRole?> FindRoleByIdAsync(string roleId);
    }
}
