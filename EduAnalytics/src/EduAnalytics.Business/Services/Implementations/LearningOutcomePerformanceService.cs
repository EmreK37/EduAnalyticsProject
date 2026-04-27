using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.Business.Services.Implementations;

public class LearningOutcomePerformanceService : ILearningOutcomePerformanceService
{
    private readonly EduAnalyticsDbContext _context;

    public LearningOutcomePerformanceService(EduAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<LearningOutcomePerformanceDto>> AnalyzeExamAsync(int examId)
    {
        // Sýnava ait test sorularýný bul
        var questions = await _context.Questions
            .Where(q => q.ExamId == examId && q.Type == Core.Enums.QuestionType.MultipleChoice)
            .Include(q => q.QuestionLearningOutcomes)
            .ThenInclude(qlo => qlo.LearningOutcome)
            .Include(q => q.StudentAnswers)
            .ToListAsync();

        var result = new List<LearningOutcomePerformanceDto>();

        // Her bir öðrenim çýktýsýna ait grup oluþtur
        var allOutcomes = questions
            .SelectMany(q => q.QuestionLearningOutcomes.Select(qlo => qlo.LearningOutcome))
            .DistinctBy(lo => lo.Id)
            .ToList();

        foreach (var outcome in allOutcomes)
        {
            var relatedQuestions = questions
                .Where(q => q.QuestionLearningOutcomes.Any(qlo => qlo.LearningOutcomeId == outcome.Id))
                .ToList();

            var answers = relatedQuestions.SelectMany(q => q.StudentAnswers).ToList();

            if (answers.Count == 0) continue;

            int correctCount = answers.Count(a => a.IsCorrect);
            double successRate = (double)correctCount / answers.Count * 100;

            result.Add(new LearningOutcomePerformanceDto
            {
                LearningOutcomeId = outcome.Id,
                OutcomeName = outcome.Name,
                Description = outcome.Description,
                RelatedQuestionCount = relatedQuestions.Count,
                TotalAnswers = answers.Count,
                CorrectAnswers = correctCount,
                SuccessRate = successRate
            });
        }

        return result.OrderByDescending(r => r.SuccessRate).ToList();
    }
}
