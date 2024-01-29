using System.ComponentModel.DataAnnotations;

namespace ASAssignment.ViewModels
{
    public class ForgotPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
