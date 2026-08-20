using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Student_Management_System.Models;

public partial class Teacher
{
    // ── DB-mapped columns ─────────────────────────────────────────────────────
    public int TeacherId { get; set; }

    /// <summary>Maps to the Name column in the Teachers table.</summary>
    public string? Name { get; set; }

    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    // ── Computed / UI-only properties (not stored in the DB) ─────────────────

    /// <summary>Alias for Name — lets views and the controller use FullName.</summary>
    [NotMapped]
    public string? FullName
    {
        get => Name;
        set => Name = value;
    }

    /// <summary>Not yet in the Teachers table — used by the UI only.</summary>
    [NotMapped]
    public string? Email { get; set; }

    [NotMapped]
    public string? Department { get; set; }

    [NotMapped]
    public string? Status { get; set; }

    [NotMapped]
    public string? Courses { get; set; }

    [NotMapped]
    public string? ProfilePicture { get; set; }

    [NotMapped]
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name)) return "T";
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][..1].ToUpper();
            return (parts[0][..1] + parts[^1][..1]).ToUpper();
        }
    }
}
