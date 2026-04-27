using EduAnalytics.Core.Enums;

namespace EduAnalytics.Business.Dtos;

/// <summary>
/// Yeni bir sınav oluşturmak için UI'dan gelen form verisi.
/// </summary>
public class ExamCreateModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public DateTime ExamDate { get; set; }
    public int CreatedByUserId { get; set; }
    public List<QuestionCreateModel> Questions { get; set; } = new();
    
    // YENİ: Sınava / derse eklenecek öğrenciler
    public List<StudentCreateModel> Students { get; set; } = new();
}

public class StudentCreateModel
{
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
}

public class QuestionCreateModel
{
    public int QuestionNumber { get; set; }
    public QuestionType Type { get; set; }
    public decimal MaxPoints { get; set; } = 1.0m;
    public string QuestionText { get; set; } = null!;

    // Test sorusu alanları
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string OptionE { get; set; } = string.Empty;
    public OptionLetter CorrectOption { get; set; }

    // Klasik soru alanları
    public string? AnswerKey { get; set; }

    public List<int> TopicIds { get; set; } = new();
}
