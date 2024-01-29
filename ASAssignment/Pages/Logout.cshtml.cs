using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASAssignment.Model;

namespace ASAssignment.Pages
{
    public class LogoutModel : PageModel
    {
		private readonly SignInManager<User> signInManager;
		public LogoutModel(SignInManager<User> signInManager)
		{
			this.signInManager = signInManager;
		}
		public void OnGet()
        {
        }
		public async Task<IActionResult> OnPostLogoutAsync()
		{
            var user = await signInManager.UserManager.GetUserAsync(User);
            if (user != null)
            {
                // Clear the StoredSessionId
                user.StoredSessionId = "";
                await signInManager.UserManager.UpdateAsync(user);
            }

            await signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToPage("Login");
		}
		public async Task<IActionResult> OnPostDontLogoutAsync()
		{
			return RedirectToPage("Index");
		}
	}
}
