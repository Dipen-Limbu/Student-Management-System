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
                return RedirectToAction("Index", "Home");
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
                if (!_context.Users.Any())
                {
                    SeedDefaultUsers();
                }
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

            return RedirectToAction("Index", "Home");
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
                _context.Users.Add(u);
            }

            _context.SaveChanges();
        }



    }
}
