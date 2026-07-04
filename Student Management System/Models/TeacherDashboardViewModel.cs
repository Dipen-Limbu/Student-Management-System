using System.Collections.Generic;

namespace Student_Management_System.Models
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = "Tom Teacher";
        public string Role { get; set; } = "Teacher";
        
        public int AssignedClasses { get; set; } = 2;
        public int TotalStudents { get; set; } = 60;
        public string TodaysAttendance { get; set; } = "86 present";
        public int PendingTasks { get; set; } = 3;

        public List<TeacherClassItem> MyClasses { get; set; } = new();
    }

    public class TeacherClassItem
    {
        public string ClassName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int EnrolledStudents { get; set; }
        public int Capacity { get; set; }
    }
}
