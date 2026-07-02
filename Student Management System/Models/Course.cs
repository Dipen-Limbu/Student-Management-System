using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? Duration { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}


// maadey vaile  first time commit gareko