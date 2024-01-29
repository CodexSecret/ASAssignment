using ASAssignment.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IDataProtector _protector;

    // Properties to hold user details
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Gender { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string ResumeFileName { get; private set; }
    public string ResumeFilePath { get; private set; }
    public string WhoAmI { get; private set; }
    public string DecryptedNRIC { get; private set; }

    public IndexModel(UserManager<User> userManager)
    {
        _userManager = userManager;
        var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
        _protector = dataProtectionProvider.CreateProtector("NRICSecretKeyProtection");
    }

    public async Task <IActionResult> OnGet()
    {
        var secureCheck = HttpContext.Session.GetString("Secured");
        if (string.IsNullOrEmpty(secureCheck) || secureCheck != "true")
        {
            return RedirectToPage("Login");
        }

        var user = await _userManager.GetUserAsync(User);

        var maxPasswordAge = TimeSpan.FromDays(90); // Example: 90 days

        if (DateTime.UtcNow - user.LastPasswordChangeDate > maxPasswordAge)
        {
            return RedirectToPage("ChangePassword");
        }

        if (user != null)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Gender = user.Gender;
            DateOfBirth = (DateTime)user.DateOfBirth;
            ResumeFileName = HttpUtility.HtmlDecode(user.ResumeFileName);
            ResumeFilePath = user.ResumeFilePath;
            WhoAmI = HttpUtility.HtmlDecode(user.WhoAmI);

            if (!string.IsNullOrWhiteSpace(user.NRIC))
            {
                DecryptedNRIC = _protector.Unprotect(user.NRIC);
            }
        }
        return Page();
    }
}
