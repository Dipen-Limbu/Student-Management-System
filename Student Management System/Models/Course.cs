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

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? CourseCode { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Description { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? Credits { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? EnrolledStudents { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? BannerColor { get; set; }
}


// maadey vaile  first time commit gareko