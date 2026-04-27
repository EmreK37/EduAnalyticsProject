namespace EduAnalytics.Core.Entities;

public class Topic
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int WeekNumber { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? LearningOutcome { get; set; }

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public ICollection<QuestionTopic> QuestionTopics { get; set; } = new List<QuestionTopic>();
}
