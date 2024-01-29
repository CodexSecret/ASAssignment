using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASAssignment.ViewModels;
using ASAssignment.Model;

namespace ASAssignment.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ChangePasswordModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public ChangePassword ChangeModel { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var secureCheck = HttpContext.Session.GetString("Secured");
            if (string.IsNullOrEmpty(secureCheck) || secureCheck != "true")
            {
                return RedirectToPage("Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await ChangePasswordAsync(user, ChangeModel.OldPassword, ChangeModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage("Index"); // Redirect to a confirmation page
        }

        private async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            // Check minimum password age policy
            var minPasswordAge = TimeSpan.FromMinutes(30); // Example: 30 minutes
            if (DateTime.UtcNow - user.LastPasswordChangeDate < minPasswordAge)
            {
                return IdentityResult.Failed(new IdentityError { Description = "You cannot change your password yet due to minimum password age policy." });
            }

            // Verify the new password is different from the current and previous two passwords
            var verifyCurrentPasswordResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword);
            var verifyPreviousPassword1Result = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PreviousPasswordHash1, newPassword);
            var verifyPreviousPassword2Result = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PreviousPasswordHash2, newPassword);

            if (verifyCurrentPasswordResult == PasswordVerificationResult.Success ||
                verifyPreviousPassword1Result == PasswordVerificationResult.Success ||
                verifyPreviousPassword2Result == PasswordVerificationResult.Success)
            {
                return IdentityResult.Failed(new IdentityError { Description = "You cannot reuse your current or last two passwords." });
            }

            // Change the password
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                // Shift the previous passwords and set the new password
                user.PreviousPasswordHash2 = user.PreviousPasswordHash1;
                user.PreviousPasswordHash1 = user.PasswordHash;

                // Update the last password change date
                user.LastPasswordChangeDate = DateTime.UtcNow;

                // Update the user with the new password history and last password change date
                await _userManager.UpdateAsync(user);
            }

            return result;
        }
    }
}
