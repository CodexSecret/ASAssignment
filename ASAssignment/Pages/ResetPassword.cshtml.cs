using ASAssignment.Model;
using ASAssignment.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Threading.Tasks;

namespace ASAssignment.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(UserManager<User> userManager, ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ResetPassword ResetModel { get; set; }

        public bool ShowErrorMessage { get; set; }
        public string ErrorMessage { get; set; }

        public IActionResult OnGet(string email, string code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return RedirectToPage("Index");
            }

            ResetModel = new ResetPassword
            {
                Email = email,
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(ResetModel.Email);
            if (user == null)
            {
                ShowErrorMessage = true;
                ErrorMessage = "User not found";
                return Page();
            }


            // Validate the reset token
            var isTokenValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", ResetModel.Code);
            if (!isTokenValid)
            {
                ShowErrorMessage = true;
                ErrorMessage = "Invalid or expired reset token";
                _logger.LogInformation("Invalid or expired reset token");
                return Page();
            }

            _logger.LogInformation("Valid token.");

            var result = await _userManager.ResetPasswordAsync(user, ResetModel.Code, ResetModel.Password);
            _logger.LogInformation("Password changed.");
            _logger.LogInformation(result.ToString());
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            ShowErrorMessage = true;
            ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            return Page();
        }
    }
}