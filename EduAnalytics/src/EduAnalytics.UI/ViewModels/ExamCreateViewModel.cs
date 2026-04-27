using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EduAnalytics.Business.Dtos;
using EduAnalytics.Business.Services.Interfaces;
using EduAnalytics.Core.Entities;

namespace EduAnalytics.UI.ViewModels;

public partial class ExamCreateViewModel : ObservableObject
{
    private readonly IExamCrudService _service;

    public event Action? ExamSaved;  // Kayıt başarılı olunca Dashboard'a dönmek için

    [ObservableProperty] private ObservableCollection<Course> _courses = new();
    [ObservableProperty] private ObservableCollection<Topic> _availableTopics = new();

    [ObservableProperty]
    private Course? _selectedCourse;

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private DateTime _examDate = DateTime.Today;

    [ObservableProperty] private ObservableCollection<QuestionEditViewModel> _questions = new();

    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;

    // YENİ EKLENEN ÖĞRENCİ VERİ GİRİŞ ALANI
    [ObservableProperty] private string _studentInputText = string.Empty;

    public ExamCreateViewModel(IExamCrudService service)
    {
        _service = service;
    }

    public async Task LoadAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            var courses = await _service.GetCoursesAsync();
            Courses = new ObservableCollection<Course>(courses);

            // Varsayılan olarak ilk dersi seç (hızlı form için)
            if (SelectedCourse == null && courses.Count > 0)
            {
                SelectedCourse = courses[0];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Veriler yüklenemedi: {ex.Message}";
        }
    }

    /// <summary>
    /// Ders seçildiğinde o dersin konularını yükler.
    /// </summary>
    partial void OnSelectedCourseChanged(Course? value)
    {
        _ = ReloadTopicsAsync();
    }

    private async Task ReloadTopicsAsync()
    {
        if (SelectedCourse == null)
        {
            AvailableTopics.Clear();
            return;
        }

        try
        {
            var topics = await _service.GetTopicsForCourseAsync(SelectedCourse.Id);
            AvailableTopics = new ObservableCollection<Topic>(topics);

            // Mevcut soruların AvailableTopics referanslarını güncelle
            foreach (var q in Questions)
            {
                q.AvailableTopics.Clear();
                foreach (var t in topics) q.AvailableTopics.Add(t);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Konular yüklenemedi: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddQuestion()
    {
        var q = new QuestionEditViewModel
        {
            QuestionNumber = Questions.Count + 1
        };
        foreach (var t in AvailableTopics) q.AvailableTopics.Add(t);
        Questions.Add(q);
    }

    [RelayCommand]
    private void RemoveQuestion(QuestionEditViewModel? q)
    {
        if (q != null)
        {
            Questions.Remove(q);
            // Numaralandırmayı yeniden yap
            for (int i = 0; i < Questions.Count; i++)
                Questions[i].QuestionNumber = i + 1;
        }
    }

    [RelayCommand]
    private void DistributePointsTo100()
    {
        if (Questions.Count == 0)
        {
            ErrorMessage = "Puan dağıtımı için en az bir soru eklemelisiniz.";
            return;
        }

        decimal pointsPerQuestion = 100m / Questions.Count;
        foreach (var q in Questions)
        {
            q.MaxPoints = Math.Round(pointsPerQuestion, 2);
        }

        SuccessMessage = $"✓ {Questions.Count} soruya ortalama {Math.Round(pointsPerQuestion, 2)} puan dağıtıldı.";
        ErrorMessage = null;
    }

    [RelayCommand]
    private void MoveQuestionUp(QuestionEditViewModel? q)
    {
        if (q == null) return;
        int index = Questions.IndexOf(q);
        if (index > 0)
        {
            Questions.Move(index, index - 1);
            // Sıralama değiştiği için numaraları güncelle
            for (int i = 0; i < Questions.Count; i++)
                Questions[i].QuestionNumber = i + 1;
        }
    }

    [RelayCommand]
    private void MoveQuestionDown(QuestionEditViewModel? q)
    {
        if (q == null) return;
        int index = Questions.IndexOf(q);
        if (index >= 0 && index < Questions.Count - 1)
        {
            Questions.Move(index, index + 1);
            // Sıralama değiştiği için numaraları güncelle
            for (int i = 0; i < Questions.Count; i++)
                Questions[i].QuestionNumber = i + 1;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        // Genel doÄŸrulamalar
        if (SelectedCourse == null)
        {
            ErrorMessage = "Ders seÃ§melisiniz.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "SÄ±nav baÅŸlÄ±ÄğÄ± boÅŸ olamaz.";
            return;
        }
        if (Questions.Count == 0)
        {
            ErrorMessage = "En az bir soru eklemelisiniz.";
            return;
        }

        // Soru bazlÄ± doÄŸrulama
        foreach (var q in Questions)
        {
            var err = q.Validate();
            if (err != null)
            {
                ErrorMessage = err;
                return;
            }
        }

        // Öğrenci Ayrıştırması
        var studentModels = new List<StudentCreateModel>();
        if (!string.IsNullOrWhiteSpace(StudentInputText))
        {
            var lines = StudentInputText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length >= 2)
                {
                    studentModels.Add(new StudentCreateModel
                    {
                        StudentNumber = parts[0],
                        FullName = parts[1],
                        ClassName = parts.Length > 2 ? parts[2] : "Tanımsız Sınıf"
                    });
                }
            }
        }

        IsSaving = true;
        try
        {
            var userId = await _service.GetDefaultUserIdAsync();
            var model = new ExamCreateModel
            {
                CourseId = SelectedCourse.Id,
                Title = Title,
                ExamDate = ExamDate,
                CreatedByUserId = userId,
                Questions = Questions.Select(q => q.ToCreateModel()).ToList(),
                Students = studentModels
            };

            var newId = await _service.CreateExamAsync(model);
            SuccessMessage = $"SÄ±nav baÅŸarÄ±yla kaydedildi. Id: {newId}";

            // Formu temizle
            Title = string.Empty;
            Questions.Clear();
            StudentInputText = string.Empty;

            ExamSaved?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"KayÄ±t hatasÄ±: {ex.Message}" +
                           (ex.InnerException != null ? $" â€” {ex.InnerException.Message}" : "");
        }
        finally
        {
            IsSaving = false;
        }
    }
}
