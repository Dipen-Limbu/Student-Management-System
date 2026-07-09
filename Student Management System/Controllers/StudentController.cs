using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            var model = new StudentDashboardViewModel
            {
                StudentName = "Sara Student",
                CourseName = "Introduction to Computer Science",
                CourseCode = "CS101",
                Semester = "1",
                Section = "Section A",
                AttendancePercentage = 57,
                AttendedClasses = 4,
                TotalClasses = 7,
                ClassTeacher = "Emily Carter",
                TeacherDepartment = "Computer Science",
                RecentAttendances = new List<AttendanceRecord>
                {
                    new AttendanceRecord { Date = "2026-07-04", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-07-03", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-07-02", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-07-01", ClassName = "CS101 - A", Status = "Present" },
                    new AttendanceRecord { Date = "2026-06-30", ClassName = "CS101 - A", Status = "Absent" },
                    new AttendanceRecord { Date = "2026-06-29", ClassName = "CS101 - A", Status = "Leave" },
                    new AttendanceRecord { Date = "2026-06-28", ClassName = "CS101 - A", Status = "Late" }
                }
            };

            return View(model);
        }

        public IActionResult MyCourse()
        {
            return View();
        }

        public IActionResult MyClass()
        {
            return View();
        }

        public IActionResult Attendance()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        // --- Admin Student Management CRUD ---

        public async Task<IActionResult> Index(string searchString, string status, string course)
        {
            var studentsQuery = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => s.FullName.Contains(searchString) || 
                                                         s.Email.Contains(searchString) || 
                                                         s.RollNo.Contains(searchString));
            }

            var students = await studentsQuery.ToListAsync();
            
            // Populate dummy fields for UI display to match design
            foreach(var s in students)
            {
                s.Course = s.Course ?? "CS101"; 
                s.Semester = s.Semester ?? "1";
                s.Status = s.Status ?? "active";
            }

            if (!string.IsNullOrEmpty(status) && status != "All statuses")
            {
                students = students.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(course) && course != "All courses")
            {
                students = students.Where(s => s.Course.Equals(course, StringComparison.OrdinalIgnoreCase)).ToList();
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
