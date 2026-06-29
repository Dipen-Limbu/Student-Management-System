using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public int? CourseId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();
}
