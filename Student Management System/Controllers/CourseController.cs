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
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateComputedProperties(Course c)
        {
            c.CourseCode = c.CourseName.Split(' ').FirstOrDefault() ?? "CRS-" + c.CourseId;
            c.Description = "This course covers the essential fundamentals of " + c.CourseName + ". Students will engage in lectures, practical exercises, and projects.";
            c.Credits = (c.CourseId % 2) + 3; // 3 or 4 credits dynamically
            c.EnrolledStudents = _context.Enrollments.Count(e => e.CourseId == c.CourseId);
            c.BannerColor = new[] { "bg-[#2546a1]", "bg-[#0b80a6]", "bg-[#543bba]", "bg-[#0f8a55]", "bg-[#ba7910]", "bg-[#ba2e2b]" }[c.CourseId % 6];
        }

        // --- Admin Course Management CRUD ---

        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.CourseName.Contains(searchString));
            }

            var courses = await query.ToListAsync();
            foreach (var course in courses)
            {
                PopulateComputedProperties(course);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
            }

            return View(courses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            PopulateComputedProperties(course);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.CourseId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Courses.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return NotFound();

            PopulateComputedProperties(course);
            return View(course);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return NotFound();

            PopulateComputedProperties(course);
            return View(course);
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

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
