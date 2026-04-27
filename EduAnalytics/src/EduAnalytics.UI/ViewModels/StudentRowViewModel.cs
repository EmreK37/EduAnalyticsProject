using System.Collections.ObjectModel;

namespace EduAnalytics.UI.ViewModels;

/// <summary>
/// DataGrid'de bir satır = bir öğrencinin tüm cevapları.
/// </summary>
public class StudentRowViewModel
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = null!;
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Sınavdaki sorularla aynı sırada hücre listesi.
    /// DataGrid kolonları `Cells[0]`, `Cells[1]`... şeklinde bağlanır.
    /// </summary>
    public ObservableCollection<AnswerCellViewModel> Cells { get; set; } = new();
}
