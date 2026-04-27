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

    public event Action? ExamSaved;

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

    // ÖĞRENCİ SEÇİMİ VE FİLTRELEME ALANLARI
    [ObservableProperty] private ObservableCollection<StudentSelectionViewModel> _allStudents = new();
    [ObservableProperty] private ObservableCollection<StudentSelectionViewModel> _filteredStudents = new();
    [ObservableProperty] private ObservableCollection<string> _availableClasses = new();

    [ObservableProperty]
    private string? _selectedClass;

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

            if (SelectedCourse == null && courses.Count > 0)
            {
                SelectedCourse = courses[0];
            }

            // Öğrencileri yükle
            var students = await _service.GetStudentsAsync();
            var studentVms = students.Select(s => new StudentSelectionViewModel
            {
                Id = s.Id,
                StudentNumber = s.StudentNumber,
                FullName = s.FullName,
                ClassName = s.ClassName,
                IsSelected = false
            }).ToList();

            AllStudents = new ObservableCollection<StudentSelectionViewModel>(studentVms);

            // Filtreleme için sınıfları al
            var classes = students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToList();
            classes.Insert(0, "Tümü");
            AvailableClasses = new ObservableCollection<string>(classes);
            SelectedClass = "Tümü";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Veriler yüklenemedi: {ex.Message}";
        }
    }

    // Sınıf filtresi değiştiğinde listeyi güncelle
    partial void OnSelectedClassChanged(string? value)
    {
        if (string.IsNullOrEmpty(value) || value == "Tümü")
        {
            FilteredStudents = new ObservableCollection<StudentSelectionViewModel>(AllStudents);
        }
        else
        {
            var filtered = AllStudents.Where(s => s.ClassName == value);
            FilteredStudents = new ObservableCollection<StudentSelectionViewModel>(filtered);
        }
    }

    [RelayCommand]
    private void SelectAllFiltered()
    {
        foreach (var student in FilteredStudents)
            student.IsSelected = true;
    }

    [RelayCommand]
    private void DeselectAllFiltered()
    {
        foreach (var student in FilteredStudents)
            student.IsSelected = false;
    }

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
            for (int i = 0; i < Questions.Count; i++)
                Questions[i].QuestionNumber = i + 1;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (SelectedCourse == null)
        {
            ErrorMessage = "Ders seçmelisiniz.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Sınav başlığı boş olamaz.";
            return;
        }
        if (Questions.Count == 0)
        {
            ErrorMessage = "En az bir soru eklemelisiniz.";
            return;
        }

        foreach (var q in Questions)
        {
            var err = q.Validate();
            if (err != null)
            {
                ErrorMessage = err;
                return;
            }
        }

        // Yeni mantıkla seçili öğrencileri alıyoruz
        var studentModels = AllStudents
            .Where(s => s.IsSelected)
            .Select(s => new StudentCreateModel
            {
                StudentNumber = s.StudentNumber,
                FullName = s.FullName,
                ClassName = s.ClassName
            })
            .ToList();

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
            SuccessMessage = $"Sınav başarıyla kaydedildi. Id: {newId}";

            // Formu temizle
            Title = string.Empty;
            Questions.Clear();

            // Seçimleri sıfırla
            foreach (var s in AllStudents) s.IsSelected = false;

            ExamSaved?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Kayıt hatası: {ex.Message}" +
                           (ex.InnerException != null ? $" — {ex.InnerException.Message}" : "");
        }
        finally
        {
            IsSaving = false;
        }
    }
}