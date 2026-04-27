namespace EduAnalytics.Core.Entities;

public class LearningOutcome
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation Properties
    public ICollection<QuestionLearningOutcome> QuestionLearningOutcomes { get; set; } = new List<QuestionLearningOutcome>();
}
