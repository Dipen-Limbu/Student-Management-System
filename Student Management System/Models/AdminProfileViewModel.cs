using System.ComponentModel.DataAnnotations;

namespace Student_Management_System.Models
{
    public class AdminProfileViewModel
    {
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string? Phone { get; set; }

        public string Role { get; set; } = "Admin";
        
        public string FullName => $"{FirstName} {LastName}";
        
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)) return "A";
                var firstInit = string.IsNullOrWhiteSpace(FirstName) ? "" : FirstName[..1];
                var lastInit = string.IsNullOrWhiteSpace(LastName) ? "" : LastName[..1];
                return (firstInit + lastInit).ToUpper();
            }
        }
    }
}
