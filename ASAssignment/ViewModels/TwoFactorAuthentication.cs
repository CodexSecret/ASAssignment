using System.ComponentModel.DataAnnotations;

namespace ASAssignment.ViewModels
{
    public class TwoFactorAuthentication
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public bool RememberMachine { get; set; }
    }
}
