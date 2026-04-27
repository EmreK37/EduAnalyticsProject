namespace EduAnalytics.Core.Entities;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public int CreatedByUserId { get; set; }

    // Navigation Properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
