using System.Collections.Generic;

namespace Student_Management_System.Models
{
    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; } = "Ada Admin";
        public string Role { get; set; } = "Admin";
        
        // Summary Cards
        public int TotalStudents { get; set; } = 42;
        public string StudentsTrend { get; set; } = "+12% vs. last month";
        public bool StudentsTrendPositive { get; set; } = true;

        public int TotalTeachers { get; set; } = 5;
        public string TeachersTrend { get; set; } = "+4% vs. last month";
        public bool TeachersTrendPositive { get; set; } = true;

        public int TotalCourses { get; set; } = 6;
        public string CoursesSubtext { get; set; } = "Across 4 departments";

        public int TotalClasses { get; set; } = 5;
        public string ClassesSubtext { get; set; } = "Running this semester";

        // Lists
        public List<ActivityItem> RecentActivity { get; set; } = new();
        public List<RecentStudentItem> RecentStudents { get; set; } = new();
    }

    public class ActivityItem
    {
        public string UserName { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }

    public class RecentStudentItem
    {
        public string Initials { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
