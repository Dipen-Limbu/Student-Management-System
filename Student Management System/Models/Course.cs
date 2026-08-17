using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_Management_System.Models;

public partial class Course
{
    // ── DB-mapped columns ─────────────────────────────────────────────────────
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? Duration { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    // ── UI-only / computed properties ─────────────────────────────────────────

    /// <summary>Short course code (maps to CourseName if no separate column exists).</summary>
    [NotMapped]
    public string? CourseCode { get; set; }

    [NotMapped]
    public string? Description { get; set; }

    [NotMapped]
    public int? Credits { get; set; }

    [NotMapped]
    public int? EnrolledStudents { get; set; }

    [NotMapped]
    public string? BannerColor { get; set; }
}
