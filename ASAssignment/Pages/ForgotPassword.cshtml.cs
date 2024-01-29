using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using ASAssignment.Model;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ASAssignment.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ForgotPasswordModel> _logger;

        [BindProperty]
        public string Email { get; set; }

        public ForgotPasswordModel(UserManager<User> userManager, IConfiguration configuration, ILogger<ForgotPasswordModel>logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync called with email: {Email}", Email);

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", Email);
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/ResetPassword",
                pageHandler: null,
                values: new { email = user.Email, code = encodedCode },
                protocol: Request.Scheme);

            await SendEmailAsync(Email, "Reset Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToPage("./ForgotPasswordConfirmation");
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
                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending email to {Email}", email);
                    throw;
                }
            }
        }

    }
}