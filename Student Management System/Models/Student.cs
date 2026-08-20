using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_Management_System.Models;

public partial class Student
{
    // ── DB-mapped columns ─────────────────────────────────────────────────────
    public int StudentId { get; set; }

    public string FullName { get; set; } = null!;

    public string RollNo { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Address { get; set; }

    public DateTime? EnrolledOn { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    // ── UI-only / computed properties ─────────────────────────────────────────

    [NotMapped]
    public string? Course { get; set; }

    [NotMapped]
    public string? Semester { get; set; }

    [NotMapped]
    public string? Status { get; set; }

    [NotMapped]
    public string? ProfilePicture { get; set; }

    [NotMapped]
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FullName)) return "S";
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][..1].ToUpper();
            return (parts[0][..1] + parts[^1][..1]).ToUpper();
        }
    }
}
