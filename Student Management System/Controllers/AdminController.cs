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
