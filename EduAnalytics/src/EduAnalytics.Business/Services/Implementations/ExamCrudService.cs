using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.Core.Entities;
using EduAnalytics.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.Business.Services.Implementations;

public class ExamCrudService : IExamCrudService
{
    private readonly EduAnalyticsDbContext _context;

    public ExamCrudService(EduAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        return await _context.Courses
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Topic>> GetTopicsForCourseAsync(int courseId)
    {
        return await _context.Topics
            .Where(t => t.CourseId == courseId)
            .OrderBy(t => t.WeekNumber)
            .ToListAsync();
    }

    // YENİ EKLENEN METOT: Tüm öğrencileri getirir
    public async Task<List<Student>> GetStudentsAsync()
    {
        return await _context.Students
            .OrderBy(s => s.StudentNumber)
            .ToListAsync();
    }

    public async Task<int> CreateExamAsync(ExamCreateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
            throw new ArgumentException("Sınav başlığı boş olamaz.", nameof(model));
        if (model.Questions.Count == 0)
            throw new ArgumentException("En az bir soru eklemeniz gerekiyor.", nameof(model));

        using var tx = await _context.Database.BeginTransactionAsync();

        var exam = new Exam
        {
            CourseId = model.CourseId,
            Title = model.Title.Trim(),
            ExamDate = model.ExamDate,
            TotalQuestions = model.Questions.Count,
            CreatedByUserId = model.CreatedByUserId
        };
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        int qNumber = 1;
        foreach (var qm in model.Questions.OrderBy(q => q.QuestionNumber))
        {
            var q = new Question
            {
                ExamId = exam.Id,
                QuestionNumber = qNumber++,
                Type = qm.Type,
                MaxPoints = qm.MaxPoints,
                QuestionText = qm.QuestionText.Trim(),
                OptionA = qm.OptionA ?? string.Empty,
                OptionB = qm.OptionB ?? string.Empty,
                OptionC = qm.OptionC ?? string.Empty,
                OptionD = qm.OptionD ?? string.Empty,
                OptionE = qm.OptionE ?? string.Empty,
                CorrectOption = qm.CorrectOption,
                AnswerKey = qm.AnswerKey
            };
            _context.Questions.Add(q);
            await _context.SaveChangesAsync();

            foreach (var topicId in qm.TopicIds.Distinct())
            {
                _context.QuestionTopics.Add(new QuestionTopic
                {
                    QuestionId = q.Id,
                    TopicId = topicId
                });
            }
        }
        await _context.SaveChangesAsync();

        // ÖĞRENCİ İŞLEMLERİ
        if (model.Students != null && model.Students.Any())
        {
            var incomingNumbers = model.Students.Select(s => s.StudentNumber).ToList();

            var existingStudents = await _context.Students
                .Where(s => incomingNumbers.Contains(s.StudentNumber))
                .ToListAsync();

            var existingNumbers = existingStudents.Select(s => s.StudentNumber).ToHashSet();

            var newStudents = model.Students
                .Where(ms => !existingNumbers.Contains(ms.StudentNumber))
                .Select(ms => new Student
                {
                    StudentNumber = ms.StudentNumber,
                    FullName = ms.FullName,
                    ClassName = string.IsNullOrWhiteSpace(ms.ClassName) ? "Tanımsız" : ms.ClassName
                })
                .ToList();

            if (newStudents.Any())
            {
                _context.Students.AddRange(newStudents);
                await _context.SaveChangesAsync();

                existingStudents.AddRange(newStudents);
            }

            var currentEnrollments = await _context.StudentCourses
                .Where(sc => sc.CourseId == model.CourseId)
                .Select(sc => sc.StudentId)
                .ToHashSetAsync();

            var newEnrollments = new List<StudentCourse>();

            foreach (var st in existingStudents)
            {
                if (!currentEnrollments.Contains(st.Id))
                {
                    newEnrollments.Add(new StudentCourse
                    {
                        CourseId = model.CourseId,
                        StudentId = st.Id
                    });
                }
            }

            if (newEnrollments.Any())
            {
                _context.StudentCourses.AddRange(newEnrollments);
                await _context.SaveChangesAsync();
            }
        }

        await tx.CommitAsync();

        return exam.Id;
    }

    public async Task<int> GetDefaultUserIdAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync();
        return user?.Id ?? throw new InvalidOperationException("Sistemde kullanıcı bulunamadı.");
    }
}