using EduAnalytics.Business.Dtos;
using EduAnalytics.Core.Entities;

namespace EduAnalytics.Business.Services.Interfaces;

/// <summary>
/// Sınav/Soru CRUD işlemleri.
/// </summary>
public interface IExamCrudService
{
    Task<List<Course>> GetCoursesAsync();
    Task<List<Topic>> GetTopicsForCourseAsync(int courseId);

    // YENİ: Sistemdeki tüm öğrencileri çekip UI'da listelemek için
    Task<List<Student>> GetStudentsAsync();

    /// <summary>
    /// Yeni sınav oluşturur. Yeni sınavın Id'sini döndürür.
    /// </summary>
    Task<int> CreateExamAsync(ExamCreateModel model);

    /// <summary>
    /// Sistemdeki ilk (default) öğretmen kullanıcısının Id'si.
    /// İleride login sistemi eklendiğinde oturumdaki kullanıcıdan alınacak.
    /// </summary>
    Task<int> GetDefaultUserIdAsync();
}
