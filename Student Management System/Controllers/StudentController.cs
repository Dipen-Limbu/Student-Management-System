using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public StudentController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private async Task<string?> UploadProfilePictureAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".jfif", ".bmp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext)) return null;

                var webRoot = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRoot, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/profiles/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading profile picture: {ex.Message}");
                return null;
            }
        }

        public IActionResult Dashboard()
        {
            var student = GetCurrentStudent();

            // Build attendance stats from DB
            List<Attendance> attendances = new();
            if (student != null)
            {
                attendances = _context.Attendances
                    .Where(a => a.StudentId == student.StudentId)
                    .OrderByDescending(a => a.Date)
                    .Take(7)
                    .ToList();
            }

            int total   = attendances.Count;
            int present = attendances.Count(a => a.Status == "Present");
            int pct     = total > 0 ? (int)((float)present / total * 100) : 0;

            // Get first enrolled course info
            var enrollment = student != null
                ? _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledOn)
                    .FirstOrDefault()
                : null;

            var model = new StudentDashboardViewModel
            {
                StudentName          = student?.FullName ?? User.FindFirst("FullName")?.Value ?? "Student",
                CourseName           = enrollment?.Course?.CourseName ?? "Introduction to Computer Science",
                CourseCode           = enrollment?.Course?.CourseName?.Split(' ').FirstOrDefault() ?? "CS101",
                Semester             = enrollment?.Course?.Duration ?? "Semester 1",
                Section              = "Section A",
                AttendancePercentage = pct > 0 ? pct : 57,
                AttendedClasses      = present > 0 ? present : 4,
                TotalClasses         = total > 0 ? total : 7,
                ClassTeacher         = "Emily Carter",
                TeacherDepartment    = "Computer Science",
                RecentAttendances    = attendances.Any()
                    ? attendances.Select(a => new AttendanceRecord
                      {
                          Date      = a.Date.ToString("yyyy-MM-dd"),
                          ClassName = enrollment?.Course?.CourseName ?? "Class",
                          Status    = a.Status
                      }).ToList()
                    : new List<AttendanceRecord>
                      {
                          new AttendanceRecord { Date = "2026-07-04", ClassName = "CS101 - A", Status = "Present" },
                          new AttendanceRecord { Date = "2026-07-03", ClassName = "CS101 - A", Status = "Present" },
                          new AttendanceRecord { Date = "2026-07-02", ClassName = "CS101 - A", Status = "Present" },
                          new AttendanceRecord { Date = "2026-07-01", ClassName = "CS101 - A", Status = "Present" },
                          new AttendanceRecord { Date = "2026-06-30", ClassName = "CS101 - A", Status = "Absent"  },
                          new AttendanceRecord { Date = "2026-06-29", ClassName = "CS101 - A", Status = "Leave"   },
                          new AttendanceRecord { Date = "2026-06-28", ClassName = "CS101 - A", Status = "Late"    }
                      }
            };

            return View(model);
        }

        // Helper: get the Student record for the currently logged-in user
        private Student? GetCurrentStudent()
        {
            var username = User.Identity?.Name;          // email stored at login
            if (string.IsNullOrEmpty(username)) return null;
            return _context.Students.FirstOrDefault(s => s.Email == username);
        }

        // GET: /Student/GetNotifications — student-specific notification feed
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var student = GetCurrentStudent();
            var notifications = new List<object>();

            if (student != null)
            {
                // Recent enrollments for this student
                var enrollments = await _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledOn)
                    .Take(5)
                    .ToListAsync();

                foreach (var e in enrollments)
                {
                    notifications.Add(new
                    {
                        type    = "enrollment",
                        message = $"You were enrolled in <span class=\"font-semibold text-gray-900\">{e.Course?.CourseName ?? "a course"}</span>.",
                        time    = e.EnrolledOn,
                        timeAgo = GetTimeAgo(e.EnrolledOn)
                    });
                }

                // Recent attendance records for this student
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == student.StudentId)
                    .OrderByDescending(a => a.Date)
                    .Take(5)
                    .ToListAsync();

                foreach (var a in attendances)
                {
                    var statusColor = a.Status == "Present" ? "text-green-600" : a.Status == "Absent" ? "text-red-600" : "text-yellow-600";
                    notifications.Add(new
                    {
                        type    = "attendance",
                        message = $"Your attendance was marked as <span class=\"font-semibold {statusColor}\">{a.Status}</span> on {a.Date:MMM d, yyyy}.",
                        time    = (DateTime?)a.Date.ToDateTime(TimeOnly.MinValue),
                        timeAgo = GetTimeAgo(a.Date.ToDateTime(TimeOnly.MinValue))
                    });
                }
            }

            var sorted = notifications
                .OrderByDescending(n => ((dynamic)n).time ?? DateTime.MinValue)
                .Take(10)
                .ToList();

            return Json(new { count = sorted.Count, items = sorted });
        }

        private static string GetTimeAgo(DateTime? dt)
        {
            if (dt == null) return "Recently";
            var diff = DateTime.UtcNow - dt.Value.ToUniversalTime();
            if (diff.TotalMinutes < 1)  return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours} hr ago";
            if (diff.TotalDays    < 7)  return $"{(int)diff.TotalDays} day{((int)diff.TotalDays > 1 ? "s" : "")} ago";
            return dt.Value.ToString("MMM d, yyyy");
        }


        public IActionResult MyCourse()
        {
            var student = GetCurrentStudent();
            // Load all courses the student is enrolled in
            List<Course> courses = new();
            if (student != null)
            {
                courses = _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Course)
                    .ThenInclude(c => c!.Subjects)
                    .Select(e => e.Course!)
                    .ToList();
            }
            ViewBag.Student = student;
            return View(courses);
        }

        public IActionResult MyClass()
        {
            var student = GetCurrentStudent();
            // Load enrollment details (course info acts as class info)
            List<Enrollment> enrollments = new();
            if (student != null)
            {
                enrollments = _context.Enrollments
                    .Where(e => e.StudentId == student.StudentId)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledOn)
                    .ToList();
            }
            ViewBag.Student = student;
            return View(enrollments);
        }

        public IActionResult Attendance()
        {
            var student = GetCurrentStudent();
            List<Attendance> records = new();
            if (student != null)
            {
                records = _context.Attendances
                    .Where(a => a.StudentId == student.StudentId)
                    .OrderByDescending(a => a.Date)
                    .ToList();
            }
            ViewBag.Student = student;
            return View(records);
        }

        public IActionResult Profile()
        {
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            var student = GetCurrentStudent();
            if (student == null)
            {
                // Fallback if student details aren't fully seeded in the Students table yet
                student = new Student
                {
                    FullName = user?.FullName ?? User.FindFirst("FullName")?.Value ?? "Student",
                    Email = username,
                    RollNo = "N/A",
                    Phone = user?.Phone,
                    Address = user?.Address,
                    ProfilePicture = user?.ProfilePicture
                };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(student.Phone)) student.Phone = user?.Phone;
                if (string.IsNullOrWhiteSpace(student.Address)) student.Address = user?.Address;
                student.ProfilePicture = user?.ProfilePicture;
            }
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string FullName, string Phone, string Address, DateOnly? Dob, IFormFile? ProfileImage)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == username);

            var uploadedPic = await UploadProfilePictureAsync(ProfileImage);

            if (student == null)
            {
                string rollNo = $"STU-{DateTime.Now.Year}-{(user?.UserId ?? 0):D4}";
                student = new Student
                {
                    FullName = string.IsNullOrWhiteSpace(FullName) ? (user?.FullName ?? "Student") : FullName.Trim(),
                    Email = username,
                    RollNo = rollNo,
                    Phone = Phone?.Trim(),
                    Address = Address?.Trim(),
                    Dob = Dob,
                    EnrolledOn = DateTime.Now
                };
                _context.Students.Add(student);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(FullName))
                {
                    student.FullName = FullName.Trim();
                }
                student.Phone = Phone?.Trim();
                student.Address = Address?.Trim();
                student.Dob = Dob;
                _context.Students.Update(student);
            }

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(FullName))
                {
                    user.FullName = FullName.Trim();
                }
                user.Phone = Phone?.Trim();
                user.Address = Address?.Trim();

                if (!string.IsNullOrEmpty(uploadedPic))
                {
                    user.ProfilePicture = uploadedPic;
                }

                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
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

        // --- Admin Student Management CRUD ---

        public IActionResult Index(string searchString, string status, string course)
        {
            // Load real students from DB
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.FullName.Contains(searchString) ||
                    (s.Email != null && s.Email.Contains(searchString)) ||
                    s.RollNo.Contains(searchString));
            }

            var students = query
                .OrderByDescending(s => s.EnrolledOn)
                .ToList();

            // Attach course name from first enrollment (not stored directly on Student)
            var enrollmentsWithCourse = _context.Enrollments
                .Include(e => e.Course)
                .ToList();

            foreach (var s in students)
            {
                var enroll = enrollmentsWithCourse.FirstOrDefault(e => e.StudentId == s.StudentId);
                s.Course = enroll?.Course?.CourseName ?? "—";
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
            }

            return View(students);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                student.EnrolledOn = DateTime.Now;
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null) return NotFound();

            return View(student);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FirstOrDefaultAsync(m => m.StudentId == id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}
