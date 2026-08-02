using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Teacher Role Dashboard Actions ──────────────────────────────────

        public IActionResult Dashboard()
        {
            var model = new TeacherDashboardViewModel
            {
                MyClasses = new List<TeacherClassItem>
                {
                    new TeacherClassItem { ClassName = "CS101 - A", Room = "Room 204", Semester = "Semester 1", EnrolledStudents = 36, Capacity = 40 },
                    new TeacherClassItem { ClassName = "DS301 - A", Room = "Room 210", Semester = "Semester 5", EnrolledStudents = 24, Capacity = 30 }
                }
            };
            return View(model);
        }

        public IActionResult MyClasses()
        {
            var courses = _context.Courses
                .Include(c => c.Enrollments)
                .ToList();

            var classes = courses.Select((c, i) => new ClassViewModel
            {
                ClassId          = c.CourseId,
                ClassName        = c.CourseName,
                CourseCode       = c.CourseName,
                Semester         = c.Duration ?? "—",
                Section          = "A",
                EnrolledStudents = c.Enrollments.Count,
                Capacity         = 40
            }).ToList();

            return View(classes);
        }

        public IActionResult MyStudents()   { return View(); }
        public IActionResult Attendance()   { return View(); }
        public IActionResult Profile()      { return View(); }
        public IActionResult ChangePassword() { return View(); }

        // ── Admin Teacher Management CRUD (now uses real DB) ────────────────

        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t =>
                    t.Name.Contains(searchString) ||
                    (t.Email      != null && t.Email.Contains(searchString)) ||
                    (t.Department != null && t.Department.Contains(searchString)));
            }

            ViewData["SearchString"] = searchString;
            return View(await query.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
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
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.TeacherId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == id);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == id);
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

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(t => t.TeacherId == id);
        }
    }
}
