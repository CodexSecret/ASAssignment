using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ASAssignment.ViewModels
{
	public class Register
	{
		[Required]
		[DataType(DataType.Text)]
		public string FirstName { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string LastName { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string Gender { get; set; }

		[Required]
		[DataType(DataType.Text)]
		[RegularExpression("^[STFG]\\d{7}[A-Z]$", ErrorMessage = "NRIC is invalid.")]
		public string NRIC { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required(ErrorMessage = "Date of birth is required.")]
		[DataType(DataType.Date)]
		[CustomDateOfBirth(ErrorMessage = "Invalid date of birth.")]
		public DateTime? DateOfBirth { get; set; }

		[Required]
        public IFormFile Resume { get; set; }

		[Required]
		[DataType(DataType.MultilineText)]
		public string WhoAmI { get; set; }
	}

	public class CustomDateOfBirthAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value is DateTime dateTimeValue)
			{
				if (dateTimeValue == DateTime.MinValue)
				{
					return new ValidationResult("Date of birth is required.");
				}

				if (dateTimeValue > DateTime.Now)
				{
					return new ValidationResult("Date of birth cannot be in the future.");
				}
				// Add any other conditions you need
			}
			return ValidationResult.Success;
		}
	}
}
