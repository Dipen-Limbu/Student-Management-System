using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public DateTime? EnrolledOn { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Student? Student { get; set; }
}
