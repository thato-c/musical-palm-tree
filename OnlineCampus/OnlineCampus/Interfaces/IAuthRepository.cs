using Microsoft.AspNetCore.Identity;
using OnlineCampus.Models;
using System.Security.Claims;

namespace OnlineCampus.Interfaces
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterUserAsync(User user, string password);
        Task SendConfirmationEmailAsync(User user, string returnUrl);
        Task AssignRoleAsync(User user, string role);
        Guid? GetUserId(ClaimsPrincipal user);
    }
}
