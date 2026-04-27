using EduAnalytics.Core.Enums;

namespace EduAnalytics.Core.Entities;

public class StudentAnswer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int StudentId { get; set; }

    /// <summary>Test sorularında seçilen şık. Klasik sorularda Empty olur.</summary>
    public OptionLetter SelectedOption { get; set; }

    /// <summary>Test sorularında doğru mu yanlış mı (otomatik hesap). Klasik için Score ≥ MaxPoints/2 ise true.</summary>
    public bool IsCorrect { get; set; }

    /// <summary>Klasik sorular için öğretmenin verdiği puan (0 ile MaxPoints arası). Test için null.</summary>
    public decimal? Score { get; set; }

    // Navigation Properties
    public Question Question { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
