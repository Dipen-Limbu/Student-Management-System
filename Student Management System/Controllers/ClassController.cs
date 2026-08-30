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
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassController(ApplicationDbContext context)
        {
            _context = context;
        }

        private ClassViewModel MapToClassViewModel(Course c, int index)
        {
            var teacherName = _context.Teachers.Skip(index % Math.Max(1, _context.Teachers.Count())).FirstOrDefault()?.Name ?? "Emily Carter";
            var enrolledCount = _context.Enrollments.Count(e => e.CourseId == c.CourseId);
            return new ClassViewModel
            {
                ClassId = c.CourseId,
                ClassName = c.CourseName,
                CourseCode = c.CourseName.Split(' ').FirstOrDefault() ?? "CRS-" + c.CourseId,
                Semester = c.Duration ?? "Semester 1",
                Section = new[] { "A", "B", "C" }[c.CourseId % 3],
                TeacherName = teacherName,
                Room = "Room " + (200 + c.CourseId),
                EnrolledStudents = enrolledCount,
                Capacity = 40
            };
        }

        // --- Admin Class Management CRUD ---

        public async Task<IActionResult> Index(string searchString)
        {
            var courses = await _context.Courses.ToListAsync();
            var classes = courses.Select((c, i) => MapToClassViewModel(c, i)).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                classes = classes.Where(c => c.ClassName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) || 
                                             (c.CourseCode != null && c.CourseCode.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) ||
                                             (c.TeacherName != null && c.TeacherName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)));
            }

            return View(classes.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassViewModel classModel)
        {
            if (ModelState.IsValid)
            {
                var course = new Course
                {
                    CourseName = classModel.ClassName ?? $"{classModel.CourseCode} - {classModel.Section}",
                    Duration = classModel.Semester ?? "Semester 1"
                };
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(classModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var classModel = MapToClassViewModel(course, course.CourseId);
            return View(classModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClassViewModel classModel)
        {
            if (id != classModel.ClassId) return NotFound();

            if (ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null) return NotFound();

                course.CourseName = classModel.ClassName;
                course.Duration = classModel.Semester;

                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(classModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var classModel = MapToClassViewModel(course, course.CourseId);
            return View(classModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var classModel = MapToClassViewModel(course, course.CourseId);
            return View(classModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
