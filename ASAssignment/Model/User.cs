using Microsoft.AspNetCore.Identity;

namespace ASAssignment.Model
{
	public class User : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Gender { get; set; }
		public string NRIC { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string ResumeFileName { get; set; }
		public string ResumeFilePath { get; set; }
		public string WhoAmI { get; set; }
        public string PreviousPasswordHash1 { get; set; }
        public string PreviousPasswordHash2 { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
		public string StoredSessionId { get; set; }
    }
}
