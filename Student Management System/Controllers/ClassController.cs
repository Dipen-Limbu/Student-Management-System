using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System.Models;
using System.Collections.Generic;
using System.Linq;

namespace Student_Management_System.Controllers
{
    [Authorize]
    public class ClassController : Controller
    {
        // Static list to simulate a database for the UI presentation
        private static List<ClassViewModel> _classes = new List<ClassViewModel>
        {
            new ClassViewModel { ClassId = 1, ClassName = "CS101 - A", CourseCode = "CS101", Semester = "1", Section = "A", TeacherName = "Emily Carter", Room = "Room 204", EnrolledStudents = 36, Capacity = 40 },
            new ClassViewModel { ClassId = 2, ClassName = "MA201 - B", CourseCode = "MA201", Semester = "3", Section = "B", TeacherName = "Michael Nguyen", Room = "Room 108", EnrolledStudents = 30, Capacity = 35 },
            new ClassViewModel { ClassId = 3, ClassName = "PH110 - A", CourseCode = "PH110", Semester = "2", Section = "A", TeacherName = "Priya Sharma", Room = "Lab 3", EnrolledStudents = 28, Capacity = 30 },
            new ClassViewModel { ClassId = 4, ClassName = "EN105 - C", CourseCode = "EN105", Semester = "1", Section = "C", TeacherName = "David Kim", Room = "Room 110", EnrolledStudents = 40, Capacity = 45 },
            new ClassViewModel { ClassId = 5, ClassName = "DS301 - A", CourseCode = "DS301", Semester = "5", Section = "A", TeacherName = "Emily Carter", Room = "Room 210", EnrolledStudents = 24, Capacity = 30 }
        };

        // --- Admin Class Management CRUD ---

        public IActionResult Index(string searchString)
        {
            var classesQuery = _classes.AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                classesQuery = classesQuery.Where(c => c.ClassName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase) || 
                                                       (c.CourseCode != null && c.CourseCode.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) ||
                                                       (c.TeacherName != null && c.TeacherName.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)));
            }

            return View(classesQuery.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClassViewModel classModel)
        {
            if (ModelState.IsValid)
            {
                classModel.ClassId = _classes.Any() ? _classes.Max(c => c.ClassId) + 1 : 1;
                // Auto-generate ClassName if missing
                if (string.IsNullOrWhiteSpace(classModel.ClassName))
                {
                    classModel.ClassName = $"{classModel.CourseCode} - {classModel.Section}";
                }
                
                _classes.Add(classModel);
                return RedirectToAction(nameof(Index));
            }
            return View(classModel);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var classModel = _classes.FirstOrDefault(c => c.ClassId == id);
            if (classModel == null) return NotFound();

            return View(classModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ClassViewModel classModel)
        {
            if (id != classModel.ClassId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingClass = _classes.FirstOrDefault(c => c.ClassId == id);
                if (existingClass != null)
                {
                    existingClass.ClassName = classModel.ClassName;
                    existingClass.CourseCode = classModel.CourseCode;
                    existingClass.Semester = classModel.Semester;
                    existingClass.Section = classModel.Section;
                    existingClass.TeacherName = classModel.TeacherName;
                    existingClass.Room = classModel.Room;
                    existingClass.EnrolledStudents = classModel.EnrolledStudents;
                    existingClass.Capacity = classModel.Capacity;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(classModel);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var classModel = _classes.FirstOrDefault(c => c.ClassId == id);
            if (classModel == null) return NotFound();

            return View(classModel);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var classModel = _classes.FirstOrDefault(c => c.ClassId == id);
            if (classModel == null) return NotFound();

            return View(classModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var classModel = _classes.FirstOrDefault(c => c.ClassId == id);
            if (classModel != null)
            {
                _classes.Remove(classModel);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
