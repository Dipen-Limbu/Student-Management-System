using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Student_Management_System.Models;
using System.Collections.Generic;

namespace Student_Management_System.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        public IActionResult Dashboard()
        {
            var model = new TeacherDashboardViewModel
            {
                MyClasses = new List<TeacherClassItem>
                {
                    new TeacherClassItem { ClassName = "CS101 - A", Room = "Room 204", Semester = "Semester 1", EnrolledStudents = 36, Capacity = 40 },
                    new TeacherClassItem { ClassName = "DS301 - A", Room = "Room 210", Semester = "Semester 5", EnrolledStudents = 24, Capacity = 30 }
                }
            };
            return View(model);
        }

        public IActionResult MyClasses() { return View(); }
        public IActionResult MyStudents() { return View(); }
        public IActionResult Attendance() { return View(); }
        public IActionResult Profile() { return View(); }
        public IActionResult ChangePassword() { return View(); }
    }
}
