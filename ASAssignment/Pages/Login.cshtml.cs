using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASAssignment.ViewModels;
using Microsoft.AspNetCore.Identity;
using ASAssignment.Model;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; // Add this for IConfiguration

namespace ASAssignment.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration, ILogger<LoginModel> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _configuration = configuration; // Initialize IConfiguration
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(LModel.Email);

                if (user != null && await userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Account locked out. Please try again later.");
                    return Page();
                }

                if (user != null)
                {
                    // Check the password here
                    var passwordCheck = await signInManager.CheckPasswordSignInAsync(user, LModel.Password, lockoutOnFailure: false);
                    if (passwordCheck.Succeeded)
                    {
                        await userManager.SetTwoFactorEnabledAsync(user, true);
                        var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
                        await SendEmailAsync(user.Email, "Your 2FA Code", $"Your 2FA code is: {token}");

                        return RedirectToPage("./TwoFactorAuthentication", new { Email = LModel.Email });
                    }
                    else
                    {
                        // If password is incorrect, add an error message
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }

            return Page();
        }


        private async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(_configuration["SMTP:SenderEmail"]));
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_configuration["SMTP:Server"], int.Parse(_configuration["SMTP:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_configuration["SMTP:Username"], _configuration["SMTP:Password"]);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("2FA email sent successfully to {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending 2FA email to {Email}", email);
                }
            }
        }
    }
}
