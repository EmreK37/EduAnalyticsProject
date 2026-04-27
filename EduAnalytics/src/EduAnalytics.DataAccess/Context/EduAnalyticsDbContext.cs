using EduAnalytics.Core.Entities;
using EduAnalytics.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduAnalytics.DataAccess.Context;

public class EduAnalyticsDbContext : DbContext
{
    public EduAnalyticsDbContext(DbContextOptions<EduAnalyticsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;
    public DbSet<Exam> Exams { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<LearningOutcome> LearningOutcomes { get; set; } = null!;
    public DbSet<QuestionTopic> QuestionTopics { get; set; } = null!;
    public DbSet<QuestionLearningOutcome> QuestionLearningOutcomes { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<StudentCourse> StudentCourses { get; set; } = null!;
    public DbSet<StudentAnswer> StudentAnswers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ───────────────────────────────────────
        // USER
        // ───────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.Role)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ───────────────────────────────────────
        // COURSE
        // ───────────────────────────────────────
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(c => c.Code).IsUnique();
            entity.Property(c => c.Description).HasMaxLength(1000);

            entity.HasOne(c => c.CreatedBy)
                  .WithMany(u => u.Courses)
                  .HasForeignKey(c => c.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────────────────────────
        // TOPIC
        // ───────────────────────────────────────
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(300);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.LearningOutcome).HasMaxLength(1000);

            entity.HasOne(t => t.Course)
                  .WithMany(c => c.Topics)
                  .HasForeignKey(t => t.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ───────────────────────────────────────
        // EXAM
        // ───────────────────────────────────────
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Exams)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.Exams)
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ───────────────────────────────────────
        // QUESTION
        // ───────────────────────────────────────
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.QuestionText).IsRequired().HasMaxLength(2000);

            entity.Property(q => q.Type)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(q => q.MaxPoints)
                  .IsRequired()
                  .HasColumnType("decimal(6,2)");

            entity.Property(q => q.OptionA).HasMaxLength(500);
            entity.Property(q => q.OptionB).HasMaxLength(500);
            entity.Property(q => q.OptionC).HasMaxLength(500);
            entity.Property(q => q.OptionD).HasMaxLength(500);
            entity.Property(q => q.AnswerKey).HasMaxLength(2000);

            entity.Property(q => q.CorrectOption)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(10);

            entity.HasOne(q => q.Exam)
                  .WithMany(e => e.Questions)
                  .HasForeignKey(q => q.ExamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ───────────────────────────────────────
        // QUESTION-TOPIC (Many-to-Many Junction Table)
        // ───────────────────────────────────────
        modelBuilder.Entity<QuestionTopic>(entity =>
        {
            entity.HasKey(qt => new { qt.QuestionId, qt.TopicId });

            entity.HasOne(qt => qt.Question)
                  .WithMany(q => q.QuestionTopics)
                  .HasForeignKey(qt => qt.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(qt => qt.Topic)
                  .WithMany(t => t.QuestionTopics)
                  .HasForeignKey(qt => qt.TopicId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ───────────────────────────────────────────────
        // QUESTION-LEARNING OUTCOME (Many-to-Many Junction Table)
        // ───────────────────────────────────────────────
        modelBuilder.Entity<QuestionLearningOutcome>(entity =>
        {
            entity.HasKey(ql => new { ql.QuestionId, ql.LearningOutcomeId });

            entity.HasOne(ql => ql.Question)
                  .WithMany(q => q.QuestionLearningOutcomes)
                  .HasForeignKey(ql => ql.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ql => ql.LearningOutcome)
                  .WithMany(l => l.QuestionLearningOutcomes)
                  .HasForeignKey(ql => ql.LearningOutcomeId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ───────────────────────────────────────
        // STUDENT COURSE (Many-to-Many Junction Table)
        // ───────────────────────────────────────
        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(sc => new { sc.StudentId, sc.CourseId });

            entity.HasOne(sc => sc.Student)
                  .WithMany(s => s.StudentCourses)
                  .HasForeignKey(sc => sc.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sc => sc.Course)
                  .WithMany(c => c.StudentCourses)
                  .HasForeignKey(sc => sc.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ───────────────────────────────────────
        // STUDENT
        // ───────────────────────────────────────
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.StudentNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(s => s.StudentNumber).IsUnique();
            entity.Property(s => s.FullName).IsRequired().HasMaxLength(150);
            entity.Property(s => s.ClassName).IsRequired().HasMaxLength(20);
        });

        // ───────────────────────────────────────
        // STUDENT ANSWER
        // ───────────────────────────────────────
        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.HasKey(sa => sa.Id);
            entity.Property(sa => sa.SelectedOption)
                  .IsRequired()
                  .HasConversion<string>()
                  .HasMaxLength(10);

            entity.Property(sa => sa.Score)
                  .HasColumnType("decimal(6,2)");

            // Aynı öğrenci aynı soruya 2 kez cevap veremez
            entity.HasIndex(sa => new { sa.QuestionId, sa.StudentId }).IsUnique();

            entity.HasOne(sa => sa.Question)
                  .WithMany(q => q.StudentAnswers)
                  .HasForeignKey(sa => sa.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sa => sa.Student)
                  .WithMany(s => s.StudentAnswers)
                  .HasForeignKey(sa => sa.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
