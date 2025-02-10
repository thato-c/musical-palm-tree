using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using OnlineCampus.Interfaces;
using OnlineCampus.Models;
using System.Text;
using System.Text.Encodings.Web;

namespace OnlineCampus.Repositories
{
    public class AuthRepository:IAuthRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthRepository> _logger;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthRepository(UserManager<User> userManager, 
                              IEmailSender emailSender,
                              ILogger<AuthRepository> logger,
                              LinkGenerator linkGenerator,
                              IHttpContextAccessor httpContextAccessor) 
        { 
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> RegisterUserAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password");
            }

            return result;
        }

        public async Task SendConfirmationEmailAsync(User user, string returnUrl)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                _logger.LogError("HttpContext is null. Cannot generate callback URL.");
            }

            var callbackUrl = _linkGenerator.GetUriByPage(
                httpContext: _httpContextAccessor.HttpContext, 
                page: "/Account/ConfirmEmail", 
                values: new { area = "Identity", userId, code, returnUrl }
            );

            if (string.IsNullOrEmpty(callbackUrl))
            {
                _logger.LogError("Failed to generate callback URL.");
                return;
            }

            string emailBody = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);
        }
    }
}
