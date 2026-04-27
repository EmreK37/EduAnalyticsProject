using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.Core.Enums;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.Business.Services.Implementations;

public class StudentPerformanceService : IStudentPerformanceService
{
    private readonly EduAnalyticsDbContext _context;

    public StudentPerformanceService(EduAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentPerformanceDto>> GetExamRankingAsync(int examId)
    {
        var questions = await _context.Questions
            .Where(q => q.ExamId == examId)
            .Include(q => q.StudentAnswers)
                .ThenInclude(a => a.Student)
            .Include(q => q.QuestionTopics)
                .ThenInclude(qt => qt.Topic)
            .ToListAsync();

        if (questions.Count == 0) return new List<StudentPerformanceDto>();

        var totalQ = questions.Count;
        var maxPossible = questions.Sum(q => q.MaxPoints);

        var studentIds = questions.SelectMany(q => q.StudentAnswers.Select(a => a.StudentId))
                                  .Distinct()
                                  .ToList();

        var results = new List<StudentPerformanceDto>();

        foreach (var studentId in studentIds)
        {
            var studentAnswers = questions
                .SelectMany(q => q.StudentAnswers
                    .Where(a => a.StudentId == studentId)
                    .Select(a => new { Question = q, Answer = a }))
                .ToList();

            if (studentAnswers.Count == 0) continue;

            var student = studentAnswers.First().Answer.Student;

            var correct = studentAnswers.Count(x => x.Answer.IsCorrect);
            var empty = studentAnswers.Count(x => x.Answer.SelectedOption == OptionLetter.Empty
                                                && x.Answer.Score == null);
            var wrong = studentAnswers.Count - correct - empty;

            decimal totalScore = studentAnswers.Sum(x => ExamAnalysisService.ComputeScore(x.Question, x.Answer));

            // Zayıf konular: Öğrencinin puan/maxpuan oranı <%50 olan konular
            var weakTopics = new List<string>();
            var topicGroups = questions
                .SelectMany(q => q.QuestionTopics.Select(qt => new { qt.Topic, Question = q }))
                .GroupBy(x => x.Topic.Id);

            foreach (var tg in topicGroups)
            {
                var topic = tg.First().Topic;
                var topicQIds = tg.Select(x => x.Question.Id).Distinct().ToHashSet();
                var relevant = studentAnswers.Where(x => topicQIds.Contains(x.Question.Id)).ToList();

                if (relevant.Count == 0) continue;

                decimal earned = relevant.Sum(x => ExamAnalysisService.ComputeScore(x.Question, x.Answer));
                decimal possible = relevant.Sum(x => x.Question.MaxPoints);

                var topicSuccess = possible > 0 ? (double)(earned / possible) * 100 : 0;
                if (topicSuccess < 50)
                    weakTopics.Add(topic.Title);
            }

            results.Add(new StudentPerformanceDto
            {
                StudentId = studentId,
                StudentNumber = student.StudentNumber,
                FullName = student.FullName,
                ClassName = student.ClassName,
                TotalQuestions = totalQ,
                CorrectAnswers = correct,
                WrongAnswers = wrong,
                EmptyAnswers = empty,
                TotalScore = Math.Round(totalScore, 2),
                MaxPossibleScore = maxPossible,
                SuccessRate = maxPossible > 0 ? Math.Round((double)(totalScore / maxPossible) * 100, 1) : 0,
                WeakTopics = weakTopics
            });
        }

        // Sıralama: Toplam puana göre azalan
        var ranked = results.OrderByDescending(r => r.TotalScore).ToList();
        for (int i = 0; i < ranked.Count; i++)
            ranked[i].ClassRank = i + 1;

        return ranked;
    }

    public async Task<StudentPerformanceDto?> GetStudentReportAsync(int examId, int studentId)
    {
        var all = await GetExamRankingAsync(examId);
        return all.FirstOrDefault(s => s.StudentId == studentId);
    }
}
