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
        private readonly IWebHostEnvironment _environment;

        public TeacherController(ApplicationDbContext context, IWebHostEnvironment environment)
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

        // Helper to get the Teacher record for the currently logged-in user
        private Teacher? GetCurrentTeacher()
        {
            if (int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                var t = _context.Teachers.FirstOrDefault(t => t.UserId == userId);
                if (t != null) return t;
            }
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return user != null ? _context.Teachers.FirstOrDefault(t => t.UserId == user.UserId) : null;
        }

        // GET: /Teacher/GetNotifications — teacher-specific notification feed
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = new List<object>();

            // Recent students enrolled (last 5)
            var recentStudents = await _context.Students
                .Where(s => s.EnrolledOn != null)
                .OrderByDescending(s => s.EnrolledOn)
                .Take(5)
                .ToListAsync();

            foreach (var s in recentStudents)
            {
                notifications.Add(new
                {
                    type    = "student",
                    message = $"New student <span class=\"font-semibold text-gray-900\">{s.FullName}</span> registered.",
                    time    = s.EnrolledOn,
                    timeAgo = GetTimeAgo(s.EnrolledOn)
                });
            }

            // Recent course enrollments
            var recentEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledOn)
                .Take(5)
                .ToListAsync();

            foreach (var e in recentEnrollments)
            {
                notifications.Add(new
                {
                    type    = "enrollment",
                    message = $"<span class=\"font-semibold text-gray-900\">{e.Student?.FullName ?? "A student"}</span> enrolled in {e.Course?.CourseName ?? "a course"}.",
                    time    = e.EnrolledOn,
                    timeAgo = GetTimeAgo(e.EnrolledOn)
                });
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

        // --- Teacher Role Dashboard Actions ---
        public IActionResult Dashboard()
        {
            var teacher = GetCurrentTeacher();

            // Load courses from DB (all courses as proxy for classes assigned)
            var courses = _context.Courses
                .Include(c => c.Enrollments)
                .ToList();

            int totalStudents = _context.Enrollments.Select(e => e.StudentId).Distinct().Count();
            int todayPresent  = _context.Attendances
                .Where(a => a.Date == DateOnly.FromDateTime(DateTime.Today) && a.Status == "Present")
                .Count();

            var model = new TeacherDashboardViewModel
            {
                TeacherName      = teacher?.Name ?? User.FindFirst("FullName")?.Value ?? "Teacher",
                Role             = "Teacher",
                AssignedClasses  = courses.Count > 0 ? courses.Count : 2,
                TotalStudents    = totalStudents > 0 ? totalStudents : 60,
                TodaysAttendance = todayPresent > 0 ? $"{todayPresent} present" : "86 present",
                PendingTasks     = 3,
                MyClasses        = courses.Take(5).Select((c, i) => new TeacherClassItem
                {
                    ClassName        = c.CourseName,
                    Room             = $"Room {200 + i + 1}",
                    Semester         = c.Duration ?? "Semester 1",
                    EnrolledStudents = c.Enrollments.Count,
                    Capacity         = 40
                }).ToList()
            };

            if (!model.MyClasses.Any())
            {
                model.MyClasses = new List<TeacherClassItem>
                {
                    new TeacherClassItem { ClassName = "CS101 - A", Room = "Room 204", Semester = "Semester 1", EnrolledStudents = 36, Capacity = 40 },
                    new TeacherClassItem { ClassName = "DS301 - A", Room = "Room 210", Semester = "Semester 5", EnrolledStudents = 24, Capacity = 30 }
                };
            }

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
        public IActionResult MyStudents()
        {
            // Load all students enrolled in any course (all students visible to teacher)
            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToList();

            // Group students with their course and attendance stats
            var studentCourseMap = enrollments
                .Where(e => e.Student != null)
                .GroupBy(e => e.StudentId)
                .Select(g =>
                {
                    var s = g.First().Student!;
                    var course = g.First().Course;

                    int total   = _context.Attendances.Count(a => a.StudentId == s.StudentId);
                    int present = _context.Attendances.Count(a => a.StudentId == s.StudentId && a.Status == "Present");
                    int pct     = total > 0 ? (int)((float)present / total * 100) : 0;

                    s.Course = course?.CourseName ?? "—";
                    ViewData[$"att_{s.StudentId}"] = pct;
                    return s;
                }).ToList();

            ViewBag.AllStudents = studentCourseMap;
            return View(studentCourseMap);
        }

        public IActionResult Attendance(int? courseId, string? date)
        {
            // Load all courses for the dropdown
            var courses = _context.Courses.OrderBy(c => c.CourseName).ToList();
            ViewBag.Courses = courses;

            int selectedCourseId = courseId ?? (courses.FirstOrDefault()?.CourseId ?? 0);
            var selectedDate = string.IsNullOrEmpty(date)
                ? DateOnly.FromDateTime(DateTime.Today)
                : DateOnly.Parse(date);

            ViewBag.SelectedCourseId = selectedCourseId;
            ViewBag.SelectedDate     = selectedDate.ToString("yyyy-MM-dd");

            // Students enrolled in the selected course
            var enrolledStudents = _context.Enrollments
                .Where(e => e.CourseId == selectedCourseId)
                .Include(e => e.Student)
                .Select(e => e.Student!)
                .ToList();

            // Existing attendance for those students on that date
            var existingAttendance = _context.Attendances
                .Where(a => a.Date == selectedDate && enrolledStudents.Select(s => s.StudentId).Contains(a.StudentId ?? 0))
                .ToList();

            ViewBag.ExistingAttendance = existingAttendance;
            return View(enrolledStudents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Attendance(int courseId, string date, Dictionary<int, string> statuses)
        {
            if (!DateOnly.TryParse(date, out var attendanceDate))
                attendanceDate = DateOnly.FromDateTime(DateTime.Today);

            foreach (var (studentId, status) in statuses)
            {
                var existing = _context.Attendances
                    .FirstOrDefault(a => a.StudentId == studentId && a.Date == attendanceDate);

                if (existing != null)
                {
                    existing.Status = status;
                    _context.Attendances.Update(existing);
                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        StudentId = studentId,
                        Date      = attendanceDate,
                        Status    = status
                    });
                }
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Attendance saved successfully!";
            return RedirectToAction(nameof(Attendance), new { courseId, date });
        }
        public IActionResult Profile() 
        { 
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            Teacher? teacher = null;
            if (int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                teacher = _context.Teachers.FirstOrDefault(t => t.UserId == userId);
            }
            if (teacher == null && user != null)
            {
                teacher = _context.Teachers.FirstOrDefault(t => t.UserId == user.UserId);
            }
            
            if (teacher == null)
            {
                teacher = new Teacher 
                { 
                    Name = user?.FullName ?? User.FindFirst("FullName")?.Value ?? "Teacher", 
                    Email = username,
                    Phone = user?.Phone,
                    Address = user?.Address,
                    ProfilePicture = user?.ProfilePicture
                };
            }
            else
            {
                teacher.Email = username;
                if (string.IsNullOrWhiteSpace(teacher.Phone)) teacher.Phone = user?.Phone;
                if (string.IsNullOrWhiteSpace(teacher.Address)) teacher.Address = user?.Address;
                teacher.ProfilePicture = user?.ProfilePicture;
            }
            
            return View(teacher); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string Name, string Phone, string Address, IFormFile? ProfileImage)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Login");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Teacher? teacher = null;
            if (int.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            }
            if (teacher == null && user != null)
            {
                teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.UserId);
            }

            var uploadedPic = await UploadProfilePictureAsync(ProfileImage);

            if (teacher == null)
            {
                teacher = new Teacher
                {
                    Name = string.IsNullOrWhiteSpace(Name) ? (user?.FullName ?? "Teacher") : Name.Trim(),
                    UserId = user?.UserId ?? 0,
                    Email = username,
                    Phone = Phone?.Trim(),
                    Address = Address?.Trim()
                };
                _context.Teachers.Add(teacher);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    teacher.Name = Name.Trim();
                }
                teacher.Phone = Phone?.Trim();
                teacher.Address = Address?.Trim();
                _context.Teachers.Update(teacher);
            }

            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    user.FullName = Name.Trim();
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

        // --- Admin Teacher Management CRUD ---

        public IActionResult Index(string searchString)
        {
            // Load teachers from DB, join with Users for email info
            var teachers = _context.Teachers
                .Join(_context.Users,
                      t => t.UserId,
                      u => u.UserId,
                      (t, u) => new Teacher
                      {
                          TeacherId = t.TeacherId,
                          Name      = t.Name,
                          UserId    = t.UserId,
                          Phone     = t.Phone ?? u.Phone,
                          Address   = t.Address ?? u.Address,
                          Email     = u.Username,
                          Status    = "active"
                      })
                .ToList();

            if (!teachers.Any())
            {
                // Fallback: show users with role Teacher
                teachers = _context.Users
                    .Where(u => u.Role == "Teacher")
                    .Select(u => new Teacher
                    {
                        TeacherId = 0,
                        Name      = u.FullName ?? u.Username,
                        Email     = u.Username,
                        Phone     = u.Phone,
                        Status    = "active"
                    }).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                teachers = teachers.Where(t =>
                    (t.Name != null && t.Name.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) ||
                    (t.Email != null && t.Email.Contains(searchString, System.StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return View(teachers);
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
                _context.Teachers.Add(teacher);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Teacher teacher)
        {
            if (id != teacher.TeacherId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Teachers.Update(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            // Attach email from Users table
            var user = _context.Users.FirstOrDefault(u => u.UserId == teacher.UserId);
            if (user != null) teacher.Email = user.Username;

            return View(teacher);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
