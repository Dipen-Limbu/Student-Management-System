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

        public IActionResult Index(string searchString, string status, string course)
        {
            var students = new List<Student>
            {
                new Student { StudentId = 1, FullName = "Liam Smith", Email = "liam.smith1@school.edu", RollNo = "2024-1000", Course = "CS101", Semester = "1", Status = "inactive" },
                new Student { StudentId = 2, FullName = "Olivia Brown", Email = "olivia.brown2@school.edu", RollNo = "2024-1001", Course = "MA201", Semester = "2", Status = "active" },
                new Student { StudentId = 3, FullName = "Noah Miller", Email = "noah.miller3@school.edu", RollNo = "2024-1002", Course = "PH110", Semester = "3", Status = "active" },
                new Student { StudentId = 4, FullName = "Emma Martinez", Email = "emma.martinez4@school.edu", RollNo = "2024-1003", Course = "EN105", Semester = "4", Status = "active" },
                new Student { StudentId = 5, FullName = "Oliver Wilson", Email = "oliver.wilson5@school.edu", RollNo = "2024-1004", Course = "BUS220", Semester = "5", Status = "active" },
                new Student { StudentId = 6, FullName = "Ava Taylor", Email = "ava.taylor6@school.edu", RollNo = "2024-1005", Course = "DS301", Semester = "6", Status = "active" },
                new Student { StudentId = 7, FullName = "Elijah Martin", Email = "elijah.martin7@school.edu", RollNo = "2024-1006", Course = "CS101", Semester = "7", Status = "active" },
                new Student { StudentId = 8, FullName = "Charlotte Thompson", Email = "charlotte.thompson8@school.edu", RollNo = "2024-1007", Course = "MA201", Semester = "1", Status = "active" },
                new Student { StudentId = 9, FullName = "William Sanchez", Email = "william.sanchez9@school.edu", RollNo = "2024-1008", Course = "PH110", Semester = "2", Status = "active" },
                new Student { StudentId = 10, FullName = "Sophia Lewis", Email = "sophia.lewis10@school.edu", RollNo = "2024-1009", Course = "EN105", Semester = "3", Status = "active" },
                new Student { StudentId = 11, FullName = "James Smith", Email = "james.smith11@school.edu", RollNo = "2024-1010", Course = "BUS220", Semester = "4", Status = "active" },
                new Student { StudentId = 12, FullName = "Amelia Brown", Email = "amelia.brown12@school.edu", RollNo = "2024-1011", Course = "DS301", Semester = "5", Status = "inactive" },
                new Student { StudentId = 13, FullName = "Benjamin Miller", Email = "benjamin.miller13@school.edu", RollNo = "2024-1012", Course = "CS101", Semester = "6", Status = "active" },
                new Student { StudentId = 14, FullName = "Isabella Martinez", Email = "isabella.martinez14@school.edu", RollNo = "2024-1013", Course = "MA201", Semester = "7", Status = "active" },
                new Student { StudentId = 15, FullName = "Lucas Wilson", Email = "lucas.wilson15@school.edu", RollNo = "2024-1014", Course = "PH110", Semester = "1", Status = "active" },
                new Student { StudentId = 16, FullName = "Mia Taylor", Email = "mia.taylor16@school.edu", RollNo = "2024-1015", Course = "EN105", Semester = "2", Status = "active" },
                new Student { StudentId = 17, FullName = "Henry Martin", Email = "henry.martin17@school.edu", RollNo = "2024-1016", Course = "BUS220", Semester = "3", Status = "active" },
                new Student { StudentId = 18, FullName = "Evelyn Thompson", Email = "evelyn.thompson18@school.edu", RollNo = "2024-1017", Course = "DS301", Semester = "4", Status = "active" },
                new Student { StudentId = 19, FullName = "Alexander Sanchez", Email = "alexander.sanchez19@school.edu", RollNo = "2024-1018", Course = "CS101", Semester = "5", Status = "active" },
                new Student { StudentId = 20, FullName = "Harper Lewis", Email = "harper.lewis20@school.edu", RollNo = "2024-1019", Course = "MA201", Semester = "6", Status = "active" }
            };

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) || 
                                               s.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) || 
                                               s.RollNo.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(status))
            {
                students = students.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(course))
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
