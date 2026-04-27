using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace EduAnalytics.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _services;

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private string _activeMenu = "Dashboard";

    public MainViewModel(IServiceProvider services)
    {
        _services = services;
        NavigateToDashboard();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        ActiveMenu = "Dashboard";
        var vm = _services.GetRequiredService<DashboardViewModel>();
        vm.OpenAnalysisRequested += OnOpenAnalysisRequested;
        _ = vm.LoadAsync();
        CurrentView = vm;
    }

    [RelayCommand]
    private void NavigateToCreateExam()
    {
        ActiveMenu = "CreateExam";
        var vm = _services.GetRequiredService<ExamCreateViewModel>();
        vm.ExamSaved += OnExamSaved;
        _ = vm.LoadAsync();
        CurrentView = vm;
    }

    private void OnOpenAnalysisRequested(int examId)
    {
        ActiveMenu = "Analiz";
        var vm = _services.GetRequiredService<ExamAnalysisViewModel>();
        vm.OpenAnswerEntryRequested += OnOpenAnswerEntryRequested;
        _ = vm.LoadAsync(examId);
        CurrentView = vm;
    }

    private void OnOpenAnswerEntryRequested(int examId)
    {
        ActiveMenu = "Cevap";
        var vm = _services.GetRequiredService<AnswerEntryViewModel>();
        _ = vm.LoadAsync(examId);
        CurrentView = vm;
    }

    private void OnExamSaved()
    {
        NavigateToDashboard();
    }
}
