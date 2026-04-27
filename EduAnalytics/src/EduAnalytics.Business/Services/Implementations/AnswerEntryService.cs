using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.Core.Entities;
using EduAnalytics.Core.Enums;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.Business.Services.Implementations;

public class AnswerEntryService : IAnswerEntryService
{
    private readonly EduAnalyticsDbContext _context;

    public AnswerEntryService(EduAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<AnswerEntryModel> LoadAsync(int examId)
    {
        var exam = await _context.Exams
            .Include(e => e.Course)
            .Include(e => e.Questions.OrderBy(q => q.QuestionNumber))
                .ThenInclude(q => q.StudentAnswers)
            .FirstOrDefaultAsync(e => e.Id == examId)
            ?? throw new InvalidOperationException($"Sınav bulunamadı: {examId}");

        // Sadece sınavın bağlı olduğu derse kayıtlı öğrencileri getir
        var students = await _context.StudentCourses
            .Where(sc => sc.CourseId == exam.CourseId)
            .Select(sc => sc.Student)
            .OrderBy(s => s.StudentNumber)
            .ToListAsync();

        var model = new AnswerEntryModel
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            CourseName = exam.Course.Name,
            Questions = exam.Questions
                .OrderBy(q => q.QuestionNumber)
                .Select(q => new AnswerEntryQuestion
                {
                    QuestionId = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    Type = q.Type,
                    MaxPoints = q.MaxPoints,
                    CorrectOption = q.CorrectOption,
                    QuestionTextPreview = q.QuestionText.Length > 60
                        ? q.QuestionText[..60] + "…"
                        : q.QuestionText
                })
                .ToList()
        };

        // Soru Id -> Cevaplar (StudentId bazlı) hızlı lookup
        var answerLookup = exam.Questions
            .SelectMany(q => q.StudentAnswers.Select(a => new { q.Id, a }))
            .ToLookup(x => x.a.StudentId);

        foreach (var s in students)
        {
            var row = new AnswerEntryStudent
            {
                StudentId = s.Id,
                StudentNumber = s.StudentNumber,
                FullName = s.FullName
            };

            foreach (var pair in answerLookup[s.Id])
            {
                row.Answers[pair.Id] = new AnswerEntryCell
                {
                    SelectedOption = pair.a.SelectedOption,
                    Score = pair.a.Score
                };
            }

            model.Students.Add(row);
        }

        return model;
    }

    public async Task SaveAsync(int examId, List<StudentAnswerUpdate> updates)
    {
        // Sınavın sorularını puan/tip bilgisiyle yükle (IsCorrect hesabı için)
        var questions = await _context.Questions
            .Where(q => q.ExamId == examId)
            .ToDictionaryAsync(q => q.Id);

        // Mevcut tüm cevapları önceden çek (update için)
        var questionIds = questions.Keys.ToList();
        var studentIds = updates.Select(u => u.StudentId).Distinct().ToList();

        var existing = await _context.StudentAnswers
            .Where(sa => questionIds.Contains(sa.QuestionId) && studentIds.Contains(sa.StudentId))
            .ToListAsync();

        var existingLookup = existing.ToDictionary(sa => (sa.QuestionId, sa.StudentId));

        foreach (var u in updates)
        {
            if (!questions.TryGetValue(u.QuestionId, out var q))
                continue;

            var isCorrect = CalculateIsCorrect(q, u);
            var key = (u.QuestionId, u.StudentId);

            if (existingLookup.TryGetValue(key, out var existingAnswer))
            {
                existingAnswer.SelectedOption = u.SelectedOption;
                existingAnswer.Score = u.Score;
                existingAnswer.IsCorrect = isCorrect;
            }
            else
            {
                // Sadece anlamlı veri içeriyorsa insert et
                if (u.SelectedOption == OptionLetter.Empty && u.Score == null)
                    continue;

                _context.StudentAnswers.Add(new StudentAnswer
                {
                    QuestionId = u.QuestionId,
                    StudentId = u.StudentId,
                    SelectedOption = u.SelectedOption,
                    Score = u.Score,
                    IsCorrect = isCorrect
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private static bool CalculateIsCorrect(Question q, StudentAnswerUpdate u)
    {
        if (q.Type == QuestionType.MultipleChoice)
            return u.SelectedOption == q.CorrectOption && u.SelectedOption != OptionLetter.Empty;

        // Klasik: Score ≥ MaxPoints/2 ise "doğru sayılır"
        return u.Score.HasValue && u.Score.Value >= q.MaxPoints / 2m;
    }
}
