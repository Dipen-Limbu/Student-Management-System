using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using System.Collections.Generic;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            var model = new StudentDashboardViewModel
            {
                StudentName = "Sara Student",
                CourseName = "Introduction to Computer Science",
                CourseCode = "CS101",
                Semester = "1",
                Section = "Section A",
                AttendancePercentage = 57,
                AttendedClasses = 4,
                TotalClasses = 7,
                ClassTeacher = "Emily Carter",
                TeacherDepartment = "Computer Science",
                RecentAttendances = new List<AttendanceRecord>
                {
                    new AttendanceRecord { Date = "2026-07-04", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-07-03", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-07-02", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-07-01", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-06-30", ClassName = "CS101 - A", Status = "Absent" },
                    new AttendanceRecord { Date = "2026-06-29", ClassName = "CS101 - A", Status = "Leave" },
                    new AttendanceRecord { Date = "2026-06-28", ClassName = "CS101 - A", Status = "Late" }
                }
            };

            return View(model);
        }

        public IActionResult MyCourse()
        {
            return View();
        }

        public IActionResult MyClass()
        {
            return View();
        }

        public IActionResult Attendance()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
