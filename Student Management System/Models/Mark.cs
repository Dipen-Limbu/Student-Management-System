using System;
using System.Collections.Generic;

namespace Student_Management_System.Models;

public partial class Mark
{
    public int MarkId { get; set; }

    public int? StudentId { get; set; }

    public int? SubjectId { get; set; }

    public decimal? MarksObtained { get; set; }

    public DateOnly? ExamDate { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Subject? Subject { get; set; }
}
