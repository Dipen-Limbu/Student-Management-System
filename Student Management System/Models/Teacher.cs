using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_Management_System.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;      // column is "Name" in DB

        public int? UserId { get; set; }               // FK to Users table

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        // --- Not mapped to DB (kept for admin list UI compatibility) ---
        [NotMapped]
        public string FullName => Name;                // alias so existing views still work

        [NotMapped]
        public string? Status { get; set; }

        [NotMapped]
        public string? Courses { get; set; }

        [NotMapped]
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "T";
                var parts = Name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0][..1].ToUpper();
                return (parts[0][..1] + parts[^1][..1]).ToUpper();
            }
        }
    }
}
