using EduAnalytics.Core.Enums;

namespace EduAnalytics.Core.Entities;

public class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = null!;

    /// <summary>Sorunun tipi — test veya klasik.</summary>
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    /// <summary>Bu sorunun tam puanı (test: genelde 1, klasik: öğretmen belirler örn. 5).</summary>
    public decimal MaxPoints { get; set; } = 1.0m;

    // Test soruları için şıklar (klasik sorularda boş string tutulur)
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string OptionE { get; set; } = string.Empty;

    /// <summary>Test soruları için doğru şık. Klasikte Empty.</summary>
    public OptionLetter CorrectOption { get; set; }

    /// <summary>Klasik soru için cevap anahtarı / notlandırma rehberi. Test soruda null.</summary>
    public string? AnswerKey { get; set; }

    // Navigation Properties
    public Exam Exam { get; set; } = null!;
    public ICollection<QuestionTopic> QuestionTopics { get; set; } = new List<QuestionTopic>();
    public ICollection<QuestionLearningOutcome> QuestionLearningOutcomes { get; set; } = new List<QuestionLearningOutcome>();
    public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
