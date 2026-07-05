using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Student_Management_System.Models;
using System.Collections.Generic;

namespace Student_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                RecentActivity = new List<ActivityItem>
                {
                    new ActivityItem { UserName = "Emily Carter", ActionText = "added a new student", TargetName = "Liam Smith", TimeAgo = "2m ago" },
                    new ActivityItem { UserName = "Ada Admin", ActionText = "created a new class", TargetName = "DS301 - A", TimeAgo = "1h ago" },
                    new ActivityItem { UserName = "Michael Nguyen", ActionText = "marked attendance", TargetName = "MA201 - B", TimeAgo = "3h ago" },
                    new ActivityItem { UserName = "Brian Chen", ActionText = "updated schedule for", TargetName = "PH110", TimeAgo = "5h ago" }
                },
                RecentStudents = new List<RecentStudentItem>
                {
                    new RecentStudentItem { Initials = "LS", Name = "Liam Smith", Email = "liam.smith1@school.edu", RollNo = "2024-1000", Course = "CS101", IsActive = false },
                    new RecentStudentItem { Initials = "OB", Name = "Olivia Brown", Email = "olivia.brown2@school.edu", RollNo = "2024-1001", Course = "MA201", IsActive = true },
                    new RecentStudentItem { Initials = "NM", Name = "Noah Miller", Email = "noah.miller3@school.edu", RollNo = "2024-1002", Course = "PH110", IsActive = true },
                    new RecentStudentItem { Initials = "EM", Name = "Emma Martinez", Email = "emma.martinez4@school.edu", RollNo = "2024-1003", Course = "EN105", IsActive = true },
                    new RecentStudentItem { Initials = "OW", Name = "Oliver Wilson", Email = "oliver.wilson5@school.edu", RollNo = "2024-1004", Course = "BUS220", IsActive = true },
                    new RecentStudentItem { Initials = "AT", Name = "Ava Taylor", Email = "ava.taylor6@school.edu", RollNo = "2024-1005", Course = "DS301", IsActive = true }
                }
            };

            return View(model);
        }

        public IActionResult Students() { return View(); }
        public IActionResult Teachers() { return View(); }
        public IActionResult Courses() { return View(); }
        public IActionResult Classes() { return View(); }
        public IActionResult Attendance() { return View(); }
        public IActionResult Reports() { return View(); }
        public IActionResult Users() { return View(); }
        public IActionResult Profile() 
        { 
            var model = new AdminProfileViewModel
            {
                FirstName = "Ada",
                LastName = "Admin",
                Email = "admin@example.com",
                Phone = "+1 555 010 1000",
                Role = "Admin"
            };
            return View(model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(AdminProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would save these changes to the database here.
                // For now, just redirect back to the profile page to simulate a successful save.
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }
            return View(model);
        }
        public IActionResult Settings() { return View(); }
    }
}
