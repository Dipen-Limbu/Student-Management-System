using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using System;
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
        public IActionResult Register(User user,
            string? FirstName, string? LastName, string? Phone)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    ModelState.AddModelError("", "Email / username is required.");
                    return View(user);
                }
                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    ModelState.AddModelError("", "Password is required.");
                    return View(user);
                }

                // Check for duplicate username
                var existingUser = _context.Users
                    .FirstOrDefault(x => x.Username == user.Username);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Username already exists.");
                    return View(user);
                }

                // --- 1. Save to Users table (with the correct Role and FullName) ---
                string fullName = $"{FirstName?.Trim()} {LastName?.Trim()}".Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    fullName = user.Username;

                User newUser = new User
                {
                    Username     = user.Username,
                    PasswordHash = user.PasswordHash,
                    Role         = string.IsNullOrWhiteSpace(user.Role) ? "Student" : user.Role,
                    FullName     = fullName
                };

                _context.Users.Add(newUser);
                _context.SaveChanges(); // get the new UserId

                // --- 2. If role is Student, also insert into Students table ---
                if (newUser.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    // Auto-generate a unique roll number: STU-<year>-<userId>
                    string rollNo = $"STU-{DateTime.Now.Year}-{newUser.UserId:D4}";

                    var student = new Student
                    {
                        FullName   = fullName,
                        RollNo     = rollNo,
                        Email      = user.Username,
                        Phone      = Phone?.Trim(),
                        EnrolledOn = DateTime.Now
                    };

                    _context.Students.Add(student);
                    _context.SaveChanges();
                }

                // --- 3. If role is Teacher, also insert into Teachers table ---
                if (newUser.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var teacher = new Teacher
                    {
                        Name    = fullName,
                        UserId  = newUser.UserId,
                        Email   = user.Username,
                        Phone   = Phone?.Trim(),
                        Address = null,
                        Department = null
                    };

                    _context.Teachers.Add(teacher);
                    _context.SaveChanges();
                }

                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Registration failed: " + ex.Message);
                return View(user);
            }
        }

        public IActionResult Index()
        {
            return RedirectToAction("Register");
        }
    }
}