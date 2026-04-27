using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.Business.Services.Implementations;

public class TopicPerformanceService : ITopicPerformanceService
{
    private readonly EduAnalyticsDbContext _context;

    public TopicPerformanceService(EduAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<TopicPerformanceDto>> AnalyzeExamAsync(int examId)
    {
        var examQuestions = await _context.Questions
            .Where(q => q.ExamId == examId)
            .Include(q => q.QuestionTopics)
                .ThenInclude(qt => qt.Topic)
            .Include(q => q.StudentAnswers)
            .ToListAsync();

        // Konu → soruları grupla
        var topicMap = examQuestions
            .SelectMany(q => q.QuestionTopics.Select(qt => new
            {
                Topic = qt.Topic,
                Question = q
            }))
            .GroupBy(x => x.Topic.Id)
            .ToList();

        var results = new List<TopicPerformanceDto>();

        foreach (var group in topicMap)
        {
            var topic = group.First().Topic;
            var questions = group.Select(g => g.Question).Distinct().ToList();

            // Puan bazlı hesap: alınan toplam puan / maksimum mümkün puan
            decimal totalEarned = 0;
            decimal totalMax = 0;
            int answerCount = 0;
            int correctCount = 0;

            foreach (var q in questions)
            {
                foreach (var a in q.StudentAnswers)
                {
                    totalEarned += ExamAnalysisService.ComputeScore(q, a);
                    totalMax += q.MaxPoints;
                    answerCount++;
                    if (a.IsCorrect) correctCount++;
                }
            }

            results.Add(new TopicPerformanceDto
            {
                TopicId = topic.Id,
                WeekNumber = topic.WeekNumber,
                TopicTitle = topic.Title,
                LearningOutcome = topic.LearningOutcome,
                RelatedQuestionCount = questions.Count,
                TotalAnswers = answerCount,
                CorrectAnswers = correctCount,
                SuccessRate = totalMax > 0 ? Math.Round((double)(totalEarned / totalMax) * 100, 1) : 0
            });
        }

        return results.OrderBy(t => t.WeekNumber).ToList();
    }

    public async Task<List<TopicPerformanceDto>> GetWeakTopicsAsync(int examId, double threshold = 60.0)
    {
        var all = await AnalyzeExamAsync(examId);
        return all.Where(t => t.SuccessRate < threshold)
                  .OrderBy(t => t.SuccessRate)
                  .ToList();
    }
}
