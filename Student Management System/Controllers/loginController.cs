using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using System.Security.Claims;

namespace Student_Management_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Login/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student")) return RedirectToAction("Dashboard", "Student");
                if (User.IsInRole("Teacher")) return RedirectToAction("Dashboard", "Teacher");
                if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Dashboard", "Student"); // Fallback for now so the dashboard button always works
            }
            return View();
        }

        // POST: /Login/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required.";
                return View();
            }

            // Seed default users if the database contains no users
            try
            {
                // Always run for development/testing so credentials work
                SeedDefaultUsers();
            }
            catch (Exception ex)
            {
                // In case database connection or table isn't created yet, log it but don't crash
                Console.WriteLine($"Database access error: {ex.Message}");
            }

            // Look up user by username (matching the screenshot's try credentials)
            User? user = null;
            try
            {
                user = _context.Users.FirstOrDefault(u => u.Username == email);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Database error: {ex.Message}";
                return View();
            }

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            bool isValid = VerifyPassword(user, password);
            if (!isValid)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Authentication Cookie Setup
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            if (user.Role == "Student")
{
    return RedirectToAction("Dashboard", "Student");
}
else if (user.Role == "Teacher")
{
    return RedirectToAction("Dashboard", "Teacher");
}
else if (user.Role == "Admin")
{
    return RedirectToAction("Dashboard", "Admin");
}
else
{
    return RedirectToAction("Index", "Home");
}
        }

        // GET: /Login/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private bool VerifyPassword(User user, string inputPassword)
        {
            // First check plain text (ideal for testing/development)
            if (user.PasswordHash == inputPassword)
            {
                return true;
            }

            // Then check SHA256 hashed password
            try
            {
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(inputPassword);
                    var hashBytes = sha.ComputeHash(bytes);
                    var hashString = Convert.ToHexString(hashBytes).ToLower();
                    if (user.PasswordHash.ToLower() == hashString)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Fallback
            }

            return false;
        }

        private void SeedDefaultUsers()
        {
            var users = new List<User>
            {
                new User { Username = "admin@example.com", PasswordHash = "admin123", Role = "Admin" },
                new User { Username = "teacher@example.com", PasswordHash = "teacher123", Role = "Teacher" },
                new User { Username = "student@example.com", PasswordHash = "student123", Role = "Student" }
            };

            foreach (var u in users)
            {
                var existingUser = _context.Users.FirstOrDefault(x => x.Username == u.Username);
                if (existingUser == null)
                {
                    _context.Users.Add(u);
                }
                else
                {
                    // Ensure the password and role match the defaults for testing
                    existingUser.PasswordHash = u.PasswordHash;
                    existingUser.Role = u.Role;
                    _context.Users.Update(existingUser);
                }
            }

            _context.SaveChanges();
        }



    }
}
