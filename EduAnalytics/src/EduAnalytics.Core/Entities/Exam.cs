namespace EduAnalytics.Core.Entities;

public class Exam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public DateTime ExamDate { get; set; }
    public int TotalQuestions { get; set; }
    public int CreatedByUserId { get; set; }

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
