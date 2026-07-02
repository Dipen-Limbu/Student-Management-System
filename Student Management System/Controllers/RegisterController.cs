using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using System.Linq;

namespace Student_Management_System.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            try
            {
                var existingUser = _context.Users
                    .FirstOrDefault(x => x.Username == user.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Username already exists.");
                    return View(user);
                }

                User newUser = new User
                {
                    Username = user.Username,
                    PasswordHash = user.PasswordHash,
                    Role = ""
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("Login", "Login");
            }
            catch
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(user);
            }
        }

        public IActionResult Index()
        {
            return RedirectToAction("Register");
        }
    }
}