using Microsoft.AspNetCore.Identity;
using OnlineCampus.Models;

namespace OnlineCampus.Interfaces
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterUserAsync(User user, string password);
        Task SendConfirmationEmailAsync(User user, string returnUrl);
    }
}
