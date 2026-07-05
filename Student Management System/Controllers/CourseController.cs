using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using System.Collections.Generic;
using System.Linq;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        // Static list to simulate a database for the UI presentation
        private static List<Course> _courses = new List<Course>
        {
            new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Introduction to Computer Science", Description = "Foundations of computing, algorithms, and programming.", Credits = 4, Duration = "16 weeks", EnrolledStudents = 128, BannerColor = "bg-[#2546a1]" },
            new Course { CourseId = 2, CourseCode = "MA201", CourseName = "Calculus II", Description = "Integration, series, and multivariable calculus.", Credits = 3, Duration = "16 weeks", EnrolledStudents = 96, BannerColor = "bg-[#0b80a6]" },
            new Course { CourseId = 3, CourseCode = "PH110", CourseName = "Physics for Engineers", Description = "Mechanics, waves, and thermodynamics.", Credits = 4, Duration = "16 weeks", EnrolledStudents = 84, BannerColor = "bg-[#543bba]" },
            new Course { CourseId = 4, CourseCode = "EN105", CourseName = "Academic English", Description = "Writing, reading and communication in academic contexts.", Credits = 2, Duration = "12 weeks", EnrolledStudents = 152, BannerColor = "bg-[#0f8a55]" },
            new Course { CourseId = 5, CourseCode = "BUS220", CourseName = "Principles of Management", Description = "Introduction to management theory and practice.", Credits = 3, Duration = "14 weeks", EnrolledStudents = 74, BannerColor = "bg-[#ba7910]" },
            new Course { CourseId = 6, CourseCode = "DS301", CourseName = "Data Structures", Description = "Trees, graphs, hashing and complexity analysis.", Credits = 4, Duration = "16 weeks", EnrolledStudents = 62, BannerColor = "bg-[#ba2e2b]" }
        };

        // --- Admin Course Management CRUD ---

        public IActionResult Index(string searchString)
        {
            var coursesQuery = _courses.AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                coursesQuery = coursesQuery.Where(c => c.CourseName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) || 
                                                       (c.CourseCode != null && c.CourseCode.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)));
            }

            return View(coursesQuery.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CourseId = _courses.Any() ? _courses.Max(c => c.CourseId) + 1 : 1;
                // Assign a random color if not selected
                if (string.IsNullOrEmpty(course.BannerColor))
                {
                    var colors = new[] { "bg-[#2546a1]", "bg-[#0b80a6]", "bg-[#543bba]", "bg-[#0f8a55]", "bg-[#ba7910]", "bg-[#ba2e2b]" };
                    course.BannerColor = colors[new System.Random().Next(colors.Length)];
                }
                
                _courses.Add(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = _courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Course course)
        {
            if (id != course.CourseId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingCourse = _courses.FirstOrDefault(c => c.CourseId == id);
                if (existingCourse != null)
                {
                    existingCourse.CourseCode = course.CourseCode;
                    existingCourse.CourseName = course.CourseName;
                    existingCourse.Description = course.Description;
                    existingCourse.Credits = course.Credits;
                    existingCourse.Duration = course.Duration;
                    existingCourse.EnrolledStudents = course.EnrolledStudents;
                    existingCourse.BannerColor = course.BannerColor;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var course = _courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();

            return View(course);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = _courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null) return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var course = _courses.FirstOrDefault(c => c.CourseId == id);
            if (course != null)
            {
                _courses.Remove(course);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
