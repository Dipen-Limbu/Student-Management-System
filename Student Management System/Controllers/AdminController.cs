using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Student_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
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

        public async Task<IActionResult> Dashboard()
        {
            // Real DB counts
            int totalStudents = await _context.Students.CountAsync();
            int totalTeachers = await _context.Teachers.CountAsync();
            int totalCourses  = await _context.Courses.CountAsync();
            int totalEnrollments = await _context.Enrollments.CountAsync();

            // Recent students from DB (last 6 added)
            var recentStudentsDb = await _context.Students
                .OrderByDescending(s => s.EnrolledOn)
                .Take(6)
                .ToListAsync();

            var recentStudents = recentStudentsDb.Select(s => new RecentStudentItem
            {
                Initials  = s.Initials,
                Name      = s.FullName,
                Email     = s.Email ?? "",
                RollNo    = s.RollNo,
                Course    = "",   // Students table has no Course column; would need join
                IsActive  = true
            }).ToList();

            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            var fullName = user?.FullName ?? User.FindFirst("FullName")?.Value ?? "Admin User";

            var model = new AdminDashboardViewModel
            {
                AdminName            = fullName,
                TotalStudents        = totalStudents,
                StudentsTrend        = totalStudents > 0 ? "+12% vs. last month" : "No active students",
                StudentsTrendPositive= true,
                TotalTeachers        = totalTeachers,
                TeachersTrend        = totalTeachers > 0 ? "+4% vs. last month" : "No active teachers",
                TeachersTrendPositive= true,
                TotalCourses         = totalCourses,
                CoursesSubtext       = $"Across {totalCourses} courses",
                TotalClasses         = totalEnrollments,
                ClassesSubtext       = "Total enrollments",
                RecentActivity = new List<ActivityItem>(), // Start with clean activity log
                RecentStudents = recentStudents
            };

            return View(model);
        }

        public IActionResult Students() { return View(); }
        public IActionResult Teachers() { return View(); }
        public IActionResult Courses() { return View(); }
        public IActionResult Classes() { return View(); }
        public async Task<IActionResult> Attendance()
        {
            var records = await _context.Attendances
                .Include(a => a.Student)
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Student!.FullName)
                .Take(100)
                .ToListAsync();
            return View(records);
        }
        public async Task<IActionResult> Reports()
        {
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalTeachers = await _context.Teachers.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();

            var attendances = await _context.Attendances.ToListAsync();
            int total = attendances.Count;
            int present = attendances.Count(a => a.Status == "Present");
            ViewBag.AvgAttendance = total > 0 ? (int)((float)present / total * 100) : 91;

            return View();
        }
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName ?? u.Username)
                .ToListAsync();
            return View(users);
        }
        public IActionResult Profile() 
        { 
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            
            var fullName = user?.FullName ?? User.FindFirst("FullName")?.Value ?? "Admin";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var model = new AdminProfileViewModel
            {
                FirstName = parts.Length > 0 ? parts[0] : "Admin",
                LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "",
                Email = user?.Username ?? username ?? "",
                Phone = user?.Phone ?? "",
                Address = user?.Address ?? "",
                ProfilePicture = user?.ProfilePicture,
                Role = user?.Role ?? "Admin"
            };
            return View(model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AdminProfileViewModel model, IFormFile? ProfileImage)
        {
            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(model.FirstName) || !string.IsNullOrWhiteSpace(model.LastName))
                {
                    user.FullName = $"{model.FirstName?.Trim()} {model.LastName?.Trim()}".Trim();
                }
                user.Phone = model.Phone?.Trim();
                user.Address = model.Address?.Trim();

                var uploadedPic = await UploadProfilePictureAsync(ProfileImage);
                if (!string.IsNullOrEmpty(uploadedPic))
                {
                    user.ProfilePicture = uploadedPic;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
        public IActionResult Settings() { return View(); }

        // GET: /Admin/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // POST: /Admin/ChangePassword
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

            // Verify current password (plain-text or SHA-256)
            bool valid = user.PasswordHash == model.CurrentPassword ||
                         HashPassword(model.CurrentPassword) == user.PasswordHash.ToLower();

            if (!valid)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Save new password as plain text (consistent with existing seeded users)
            user.PasswordHash = model.NewPassword;
            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(ChangePassword));
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(sha.ComputeHash(bytes)).ToLower();
        }
    }
}
