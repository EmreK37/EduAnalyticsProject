using EduAnalytics.Core.Enums;

namespace EduAnalytics.Business.Dtos;

/// <summary>
/// Cevap girişi ekranı için tüm veriyi bir arada taşır.
/// </summary>
public class AnswerEntryModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = null!;
    public string CourseName { get; set; } = null!;
    public List<AnswerEntryQuestion> Questions { get; set; } = new();
    public List<AnswerEntryStudent> Students { get; set; } = new();
}

public class AnswerEntryQuestion
{
    public int QuestionId { get; set; }
    public int QuestionNumber { get; set; }
    public QuestionType Type { get; set; }
    public decimal MaxPoints { get; set; }
    public OptionLetter CorrectOption { get; set; }
    public string QuestionTextPreview { get; set; } = string.Empty;
}

public class AnswerEntryStudent
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = null!;
    public string FullName { get; set; } = null!;
    /// <summary>QuestionId → mevcut cevap (varsa).</summary>
    public Dictionary<int, AnswerEntryCell> Answers { get; set; } = new();
}

public class AnswerEntryCell
{
    public OptionLetter SelectedOption { get; set; } = OptionLetter.Empty;
    public decimal? Score { get; set; }
}

/// <summary>
/// Kaydederken UI'dan servise gönderilen tek bir hücre güncellemesi.
/// </summary>
public class StudentAnswerUpdate
{
    public int QuestionId { get; set; }
    public int StudentId { get; set; }
    public OptionLetter SelectedOption { get; set; }
    public decimal? Score { get; set; }
}
