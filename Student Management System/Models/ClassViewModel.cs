using System.ComponentModel.DataAnnotations;

namespace Student_Management_System.Models
{
    public class ClassViewModel
    {
        public int ClassId { get; set; }

        [Required, StringLength(100)]
        public string ClassName { get; set; } = null!; // e.g., "CS101 - A"

        [Required, StringLength(20)]
        public string CourseCode { get; set; } = null!; // e.g., "CS101"

        [StringLength(20)]
        public string? Semester { get; set; } // e.g., "1"

        [StringLength(10)]
        public string? Section { get; set; } // e.g., "A"

        [StringLength(100)]
        public string? TeacherName { get; set; } // e.g., "Emily Carter"

        [StringLength(50)]
        public string? Room { get; set; } // e.g., "Room 204"

        public int EnrolledStudents { get; set; } // e.g., 36

        public int Capacity { get; set; } // e.g., 40
        
        // Helper to calculate progress width for the bar
        public int EnrollmentPercentage => Capacity > 0 ? (int)((float)EnrolledStudents / Capacity * 100) : 0;
    }
}
