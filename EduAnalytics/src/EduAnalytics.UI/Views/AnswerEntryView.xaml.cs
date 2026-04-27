using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using EduAnalytics.Core.Enums;
using EduAnalytics.UI.ViewModels;

namespace EduAnalytics.UI.Views;

public partial class AnswerEntryView : UserControl
{
    public AnswerEntryView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is AnswerEntryViewModel old)
            old.QuestionsLoaded -= BuildColumns;

        if (e.NewValue is AnswerEntryViewModel vm)
        {
            vm.QuestionsLoaded += BuildColumns;
            if (vm.Questions.Any())
                BuildColumns();
        }
    }

    /// <summary>
    /// Sorular yüklendikten sonra DataGrid kolonlarını dinamik olarak oluşturur.
    /// Test sorusu → ComboBox (A/B/C/D/Boş). Klasik soru → TextBox (numerik).
    /// </summary>
    private void BuildColumns()
    {
        if (DataContext is not AnswerEntryViewModel vm) return;

        AnswersGrid.Columns.Clear();

        // Öğrenci bilgi sütunları (sabit / donmuş)
        AnswersGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Numara",
            Binding = new Binding("StudentNumber"),
            IsReadOnly = true,
            Width = new DataGridLength(90),
            CellStyle = MakeCellStyle(bold: true, background: "#F3F4F6")
        });

        AnswersGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Öğrenci",
            Binding = new Binding("FullName"),
            IsReadOnly = true,
            Width = new DataGridLength(180),
            CellStyle = MakeCellStyle(bold: true, background: "#F3F4F6")
        });

        // Soru sütunları
        for (int i = 0; i < vm.Questions.Count; i++)
        {
            var q = vm.Questions[i];
            var header = q.Type == QuestionType.MultipleChoice
                ? $"S{q.QuestionNumber}\n(Test {q.MaxPoints:0.#}p)"
                : $"S{q.QuestionNumber}\n(Klasik {q.MaxPoints:0.#}p)";

            DataGridColumn column;

            if (q.Type == QuestionType.MultipleChoice)
            {
                var comboCol = new DataGridComboBoxColumn
                {
                    Header = header,
                    ItemsSource = vm.OptionLetters,
                    SelectedItemBinding = new Binding($"Cells[{i}].SelectedOption")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    Width = new DataGridLength(70)
                };
                column = comboCol;
            }
            else
            {
                // Klasik: TextBox ile puan girişi
                var textCol = new DataGridTextColumn
                {
                    Header = header,
                    Binding = new Binding($"Cells[{i}].Score")
                    {
                        StringFormat = "N1",
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        TargetNullValue = string.Empty
                    },
                    Width = new DataGridLength(80),
                    ElementStyle = MakeTextElementStyle(background: "#FEF9C3"),
                    EditingElementStyle = MakeEditingStyle()
                };
                column = textCol;
            }

            AnswersGrid.Columns.Add(column);
        }
    }

    private static Style MakeCellStyle(bool bold = false, string? background = null)
    {
        var style = new Style(typeof(DataGridCell));
        if (bold)
            style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        if (background != null)
            style.Setters.Add(new Setter(Control.BackgroundProperty,
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(background))));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 4, 8, 4)));
        return style;
    }

    private static Style MakeTextElementStyle(string? background = null)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(8, 4, 8, 4)));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
        if (background != null)
            style.Setters.Add(new Setter(TextBlock.BackgroundProperty,
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(background))));
        return style;
    }

    private static Style MakeEditingStyle()
    {
        var style = new Style(typeof(TextBox));
        style.Setters.Add(new Setter(TextBox.TextAlignmentProperty, TextAlignment.Center));
        style.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(6, 2, 6, 2)));
        return style;
    }
}
