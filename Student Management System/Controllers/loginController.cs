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
        public async Task<IActionResult> Login()
        {
            // Always clear any existing authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View();
        }

        // POST: /Login/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and Password are required.";
                return View();
            }

            // Seed default users if database is empty
            if (!_context.Users.Any())
            {
                SeedDefaultUsers();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            if (!VerifyPassword(user, password))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private bool VerifyPassword(User user, string inputPassword)
        {
            if (user.PasswordHash == inputPassword)
                return true;

            try
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = System.Text.Encoding.UTF8.GetBytes(inputPassword);
                var hash = Convert.ToHexString(sha.ComputeHash(bytes)).ToLower();

                return user.PasswordHash.ToLower() == hash;
            }
            catch
            {
                return false;
            }
        }

        private void SeedDefaultUsers()
        {
            _context.Users.AddRange(
                new User
                {
                    Username = "admin@example.com",
                    PasswordHash = "admin123",
                    Role = "Admin"
                },
                new User
                {
                    Username = "teacher@example.com",
                    PasswordHash = "teacher123",
                    Role = "Teacher"
                },
                new User
                {
                    Username = "student@example.com",
                    PasswordHash = "student123",
                    Role = "Student"
                });

            _context.SaveChanges();
        }
    }
}