using ASAssignment.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ASAssignment.Pages
{
    [Authorize]
    public class AuditLogModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthDbContext _context;

        public IList<AuditLog> AuditLogEntries { get; set; }

        public AuditLogModel(UserManager<User> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task <IActionResult> OnGetAsync()
        {
            var secureCheck = HttpContext.Session.GetString("Secured");
            if (string.IsNullOrEmpty(secureCheck) || secureCheck != "true")
            {
                return RedirectToPage("Login");
            }
            var user = await _userManager.GetUserAsync(User);

            var maxPasswordAge = TimeSpan.FromDays(90);

            if (DateTime.UtcNow - user.LastPasswordChangeDate > maxPasswordAge)
            {
                return RedirectToPage("ChangePassword");
            }
            AuditLogEntries = await _context.AuditLogEntries.ToListAsync();

            return Page();
        }
    }
}
