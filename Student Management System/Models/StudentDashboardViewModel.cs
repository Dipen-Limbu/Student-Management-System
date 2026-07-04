using System.Collections.Generic;

namespace Student_Management_System.Models
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "Sara Student";
        public string CourseName { get; set; } = "Introduction to Computer Science";
        public string CourseCode { get; set; } = "CS101";
        public string Semester { get; set; } = "1";
        public string Section { get; set; } = "Section A";
        public int AttendancePercentage { get; set; } = 57;
        public int AttendedClasses { get; set; } = 4;
        public int TotalClasses { get; set; } = 7;
        public string ClassTeacher { get; set; } = "Emily Carter";
        public string TeacherDepartment { get; set; } = "Computer Science";
        
        public List<AttendanceRecord> RecentAttendances { get; set; } = new();
    }

    public class AttendanceRecord
    {
        public string Date { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Present, Absent, Leave, Late
    }
}
