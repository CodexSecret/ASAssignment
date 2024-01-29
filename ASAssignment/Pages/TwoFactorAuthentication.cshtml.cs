using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASAssignment.ViewModels;
using Microsoft.AspNetCore.Identity;
using ASAssignment.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore;

namespace ASAssignment.Pages
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        [BindProperty]
        public TwoFactorAuthentication TwoFactorModel { get; set; } = new TwoFactorAuthentication();

        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;
        private readonly AuthDbContext _context;

        public TwoFactorAuthenticationModel(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<TwoFactorAuthenticationModel> logger, AuthDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string email, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Login");
            }

            TwoFactorModel.Email = email;
            TwoFactorModel.RememberMachine = rememberMe;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(TwoFactorModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "An error occurred.");
                return Page();
            }

            _logger.LogInformation("Attempting 2FA for user {Email} with code {Code}", TwoFactorModel.Email, TwoFactorModel.Code);

            var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", TwoFactorModel.Code);
            if (result)
            {
                // Generate a new session ID
                var newSessionId = Guid.NewGuid().ToString();

                // Check if the StoredSessionId in the database is different from the new session ID
                if (!string.IsNullOrEmpty(user.StoredSessionId) && user.StoredSessionId != newSessionId)
                {
                    // Detected login from another device or browser tab
                    ModelState.AddModelError(string.Empty, "Your account is logged in from another device or browser tab.");
                    _logger.LogInformation("User {Email} attempted to log in, but is already logged in from another device or browser tab.", TwoFactorModel.Email);
                    return Page();
                }

                // Set the new session ID to the user and update in the database
                user.StoredSessionId = newSessionId;
                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, TwoFactorModel.RememberMachine);

                    // Store the session ID and other relevant information in HttpContext.Session
                    HttpContext.Session.SetString("Secured", "true");
                    HttpContext.Session.SetString("SessionID", newSessionId);
                    HttpContext.Session.SetString("User", user.Id.ToString());

                    var auditLog = new AuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        Action = "Login",
                        Details = $"Login successful for email: {TwoFactorModel.Email}",
                        UserId = user.Id // Assuming AuditLog has a UserId property
                    };

                    _context.AuditLogEntries.Add(auditLog);
                    await _context.SaveChangesAsync();

                    return RedirectToPage("/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to update user session.");
                    var auditLog = new AuditLog
                    {
                        Timestamp = DateTime.UtcNow,
                        Action = "2FA Verification Failed",
                        Details = $"Failed to update session after 2FA verification for email: {TwoFactorModel.Email}",
                        UserId = user?.Id ?? "Unknown"
                    };

                    _context.AuditLogEntries.Add(auditLog);
                    await _context.SaveChangesAsync();
                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid authentication code.");
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    Action = "Login Failed",
                    Details = $"Invalid login for email: {TwoFactorModel.Email}",
                    UserId = user?.Id ?? "Unknown"
                };

                _context.AuditLogEntries.Add(auditLog);
                await _context.SaveChangesAsync();
                return Page();
            }
        }

    }
}
