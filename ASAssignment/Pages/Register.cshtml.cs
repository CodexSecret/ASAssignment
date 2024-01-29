using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASAssignment.ViewModels;
using ASAssignment.Model;
using Microsoft.AspNetCore.DataProtection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Web;

namespace ASAssignment.Pages
{
    public class MyObject
    {
        public string success { get; set; }
    }


    public class RegisterModel : PageModel
    {
        private UserManager<User> userManager { get; }
        private readonly IConfiguration config;
        private ILogger<RegisterModel> logger { get; set; }

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<User> userManager, IConfiguration config, ILogger<RegisterModel> logger)
        {
            this.userManager = userManager;
            this.config = config;
            this.logger = logger;
        }

        public void OnGet()
        {
            ViewData["ReCaptchaSiteKey"] = config["captchaSiteKey"];
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var allowedExtensions = new[] { ".pdf", ".docx" };
            if (RModel.Resume != null && RModel.Resume.Length > 0)
            {
                var extension = Path.GetExtension(RModel.Resume.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("RModel.Resume", "Only .pdf and .docx files are allowed.");
                }
            }
            else
            {
                ModelState.AddModelError("RModel.Resume", "Uploading a resume file is required.");
            }
            if (ModelState.IsValid)
            {
                if (!Regex.IsMatch(RModel.NRIC, "^[STFG]\\d{7}[A-Z]$"))
                {
                    ModelState.AddModelError("RModel.NRIC", "NRIC format is invalid.");
                }
                if (!Regex.IsMatch(RModel.Password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[$@$!%*?&])[A-Za-z\\d$@$!%*?&]{8,10}"))
                {
                    ModelState.AddModelError("RModel.Password", "Password format is invalid.");
                }
                if (!Regex.IsMatch(RModel.Email, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"))
                {
                    ModelState.AddModelError("RModel.Email", "Email format is invalid.");
                }
                if (RModel.Resume == null || RModel.Resume.Length == 0)
                {
                    ModelState.AddModelError("RModel.Resume", "Uploading a resume file is required.");
                }
                var validCaptcha = await ValidateCaptcha();
                if (!validCaptcha)
                {
                    ModelState.AddModelError("", "reCAPTCHA validation failed. Please try again.");
                }
                var uploadsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDirectoryPath))
                {
                    Directory.CreateDirectory(uploadsDirectoryPath);
                }

                var resumeFileName = Guid.NewGuid().ToString() + Path.GetExtension(RModel.Resume.FileName);
                var filePath = Path.Combine(uploadsDirectoryPath, resumeFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await RModel.Resume.CopyToAsync(fileStream);
                }

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("NRICSecretKeyProtection");

                var user = new User()
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FirstName = RModel.FirstName,
                    LastName = RModel.LastName,
                    Gender = RModel.Gender,
                    NRIC = protector.Protect(RModel.NRIC),
                    DateOfBirth = RModel.DateOfBirth,
                    ResumeFileName = HttpUtility.HtmlEncode(RModel.Resume.FileName),
                    ResumeFilePath = resumeFileName,
                    WhoAmI = HttpUtility.HtmlEncode(RModel.WhoAmI),
                    PreviousPasswordHash1 = string.Empty,
                    PreviousPasswordHash2 = string.Empty,
                    StoredSessionId = string.Empty,
                    LastPasswordChangeDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToPage("Login");
                }

                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError("", "Email is already used.");
                    }
                    else
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return Page();
        }

        public async Task<bool> ValidateCaptcha()
        {
            string captchaResponse = Request.Form["g-recaptcha-response"];
            string secretKey = config["captchaSecretKey"];
            string apiUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={Uri.EscapeDataString(secretKey)}&response={Uri.EscapeDataString(captchaResponse)}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        MyObject jsonObject = JsonConvert.DeserializeObject<MyObject>(jsonResponse);

                        return Convert.ToBoolean(jsonObject.success);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
