using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Static list to simulate a database for the UI presentation
        // Replace with ApplicationDbContext _context when the Teacher table is created in the database.
        private static List<Teacher> _teachers = new List<Teacher>
        {
            new Teacher { TeacherId = 1, Name = "Emily Carter", Email = "emily.carter@school.edu", Phone = "+1 555 200 1201", Department = "Computer Science", Status = "active", Courses = "CS101, DS301" },
            new Teacher { TeacherId = 2, Name = "Michael Nguyen", Email = "michael.nguyen@school.edu", Phone = "+1 555 200 1202", Department = "Mathematics", Status = "active", Courses = "MA201" },
            new Teacher { TeacherId = 3, Name = "Priya Sharma", Email = "priya.sharma@school.edu", Phone = "+1 555 200 1203", Department = "Physics", Status = "active", Courses = "PH110" },
            new Teacher { TeacherId = 4, Name = "David Kim", Email = "david.kim@school.edu", Phone = "+1 555 200 1204", Department = "English", Status = "active", Courses = "EN105" },
            new Teacher { TeacherId = 5, Name = "Sofia Rossi", Email = "sofia.rossi@school.edu", Phone = "+1 555 200 1205", Department = "Business", Status = "inactive", Courses = "BUS220" }
        };
        // --- Teacher Role Dashboard Actions ---
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

        public IActionResult MyClasses()
        {
            // Load all courses from the database with their enrollment counts
            var courses = _context.Courses
                .Include(c => c.Enrollments)
                .ToList();

            var classes = courses.Select((c, i) => new ClassViewModel
            {
                ClassId      = c.CourseId,
                ClassName    = c.CourseName,
                CourseCode   = c.CourseName, // use CourseName as code since no separate code column
                Semester     = c.Duration ?? "—",
                Section      = "A",
                EnrolledStudents = c.Enrollments.Count,
                Capacity     = 40  // default capacity; update when Capacity column exists
            }).ToList();

            return View(classes);
        }
        public IActionResult MyStudents() { return View(); }
        public IActionResult Attendance() { return View(); }
        public IActionResult Profile() 
        { 
            Teacher? teacher = null;
            if (int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                teacher = _context.Teachers.FirstOrDefault(t => t.UserId == userId);
            }
            
            if (teacher == null)
            {
                teacher = new Teacher 
                { 
                    Name = User.FindFirst("FullName")?.Value ?? "Teacher", 
                    Email = User.Identity?.Name 
                };
            }
            else
            {
                teacher.Email = User.Identity?.Name;
            }
            
            return View(teacher); 
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.FindFirstValue(ClaimTypes.Name);
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User account not found.");
                return View(model);
            }

            bool valid = user.PasswordHash == model.CurrentPassword ||
                         HashSha256(model.CurrentPassword) == user.PasswordHash.ToLower();

            if (!valid)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            user.PasswordHash = model.NewPassword;
            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(ChangePassword));
        }

        private static string HashSha256(string input)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input))).ToLower();
        }

        // --- Admin Teacher Management CRUD ---

        public IActionResult Index(string searchString)
        {
            var teachersQuery = _teachers.AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                teachersQuery = teachersQuery.Where(t => t.FullName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) || 
                                                         (t.Email != null && t.Email.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) ||
                                                         (t.Department != null && t.Department.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)));
            }

            return View(teachersQuery.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                teacher.TeacherId = _teachers.Any() ? _teachers.Max(t => t.TeacherId) + 1 : 1;
                _teachers.Add(teacher);
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = _teachers.FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Teacher teacher)
        {
            if (id != teacher.TeacherId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingTeacher = _teachers.FirstOrDefault(t => t.TeacherId == id);
                if (existingTeacher != null)
                {
                    existingTeacher.Name = teacher.FullName;
                    existingTeacher.Email = teacher.Email;
                    existingTeacher.Phone = teacher.Phone;
                    existingTeacher.Department = teacher.Department;
                    existingTeacher.Status = teacher.Status;
                    existingTeacher.Courses = teacher.Courses;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = _teachers.FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = _teachers.FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var teacher = _teachers.FirstOrDefault(t => t.TeacherId == id);
            if (teacher != null)
            {
                _teachers.Remove(teacher);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
