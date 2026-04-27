using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.Core.Enums;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.Business.Services.Implementations;

public class DistractorAnalysisService : IDistractorAnalysisService
{
    private readonly EduAnalyticsDbContext _context;

    public DistractorAnalysisService(EduAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestionAnalysisDto>> AnalyzeExamAsync(int examId)
    {
        var questions = await _context.Questions
            .Where(q => q.ExamId == examId)
            .Include(q => q.StudentAnswers)
            .Include(q => q.QuestionTopics)
                .ThenInclude(qt => qt.Topic)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync();

        var results = new List<QuestionAnalysisDto>();

        foreach (var q in questions)
        {
            var answers = q.StudentAnswers.ToList();
            var total = answers.Count;
            var correct = answers.Count(a => a.IsCorrect);
            var empty = answers.Count(a => a.SelectedOption == OptionLetter.Empty && a.Score == null);
            var wrong = total - correct - empty;

            var dto = new QuestionAnalysisDto
            {
                QuestionId = q.Id,
                QuestionNumber = q.QuestionNumber,
                QuestionText = q.QuestionText,
                QuestionType = q.Type.ToString(),
                MaxPoints = q.MaxPoints,
                CorrectOption = q.Type == QuestionType.MultipleChoice ? q.CorrectOption.ToString() : "—",
                TotalAnswers = total,
                CorrectCount = correct,
                WrongCount = wrong,
                EmptyCount = empty,
                SuccessRate = total > 0 ? (double)correct / total * 100 : 0,
                LinkedTopicTitles = q.QuestionTopics.Select(qt => qt.Topic.Title).ToList()
            };

            if (q.Type == QuestionType.MultipleChoice)
            {
                // Test sorusu: Şık dağılımı + çeldirici tespiti
                dto.OptionACount = answers.Count(a => a.SelectedOption == OptionLetter.A);
                dto.OptionBCount = answers.Count(a => a.SelectedOption == OptionLetter.B);
                dto.OptionCCount = answers.Count(a => a.SelectedOption == OptionLetter.C);
                dto.OptionDCount = answers.Count(a => a.SelectedOption == OptionLetter.D);
                dto.OptionECount = answers.Count(a => a.SelectedOption == OptionLetter.E);
                dto.AverageScore = total > 0
                    ? (decimal)correct / total * q.MaxPoints
                    : 0;

                // Çeldirici tespiti: Yanlış yapanların en çok seçtiği şık
                var wrongAnswers = answers.Where(a => !a.IsCorrect
                                                   && a.SelectedOption != OptionLetter.Empty)
                                          .ToList();
                if (wrongAnswers.Count > 0)
                {
                    var distractorGroup = wrongAnswers
                        .GroupBy(a => a.SelectedOption)
                        .Select(g => new { Option = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .First();

                    dto.StrongestDistractorOption = distractorGroup.Option.ToString();
                    dto.StrongestDistractorCount = distractorGroup.Count;
                    dto.StrongestDistractorRate = Math.Round((double)distractorGroup.Count / wrongAnswers.Count * 100, 1);
                }
            }
            else
            {
                // Klasik soru: Çeldirici yok, puan ortalaması anlamlı
                var scored = answers.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                dto.AverageScore = scored.Any() ? Math.Round(scored.Average(), 2) : 0;
                dto.SuccessRate = scored.Any()
                    ? Math.Round((double)(scored.Average() / q.MaxPoints) * 100, 1)
                    : 0;
                dto.StrongestDistractorOption = null;
                dto.StrongestDistractorRate = 0;
            }

            results.Add(dto);
        }

        return results;
    }

    public async Task<List<QuestionAnalysisDto>> GetStrongDistractorsAsync(int examId, double minDistractorRate = 50.0)
    {
        var all = await AnalyzeExamAsync(examId);
        return all.Where(q => q.QuestionType == "MultipleChoice"
                           && q.StrongestDistractorRate >= minDistractorRate)
                  .OrderByDescending(q => q.StrongestDistractorRate)
                  .ToList();
    }
}
