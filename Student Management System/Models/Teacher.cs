using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_Management_System.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        
        [Required, StringLength(100)]
        public string FullName { get; set; } = null!;
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(100)]
        public string? Department { get; set; }
        
        [StringLength(20)]
        public string? Status { get; set; }
        
        [StringLength(255)]
        public string? Courses { get; set; }

        [NotMapped]
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName)) return "T";
                var parts = FullName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0][..1].ToUpper();
                return (parts[0][..1] + parts[^1][..1]).ToUpper();
            }
        }
    }
}
