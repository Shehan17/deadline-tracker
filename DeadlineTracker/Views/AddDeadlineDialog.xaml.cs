using DeadlineTracker.Models;
using System.Windows;

namespace DeadlineTracker.Views;

public partial class AddDeadlineDialog : Window
{
    public AddDeadlineDialog()
    {
        InitializeComponent();
        DeadlineDatePicker.SelectedDate = DateTime.Today;
    }

    public DeadlineItem? CreatedDeadline { get; private set; }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryCreateDeadline(out DeadlineItem? deadline))
            return;

        CreatedDeadline = deadline;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool TryCreateDeadline(out DeadlineItem? deadline)
    {
        deadline = null;
        ValidationTextBlock.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            ValidationTextBlock.Text = "Please enter a title.";
            return false;
        }

        if (DeadlineDatePicker.SelectedDate is null)
        {
            ValidationTextBlock.Text = "Please select a deadline date.";
            return false;
        }

        if (!TimeSpan.TryParse(DeadlineTimeTextBox.Text, out TimeSpan deadlineTime))
        {
            ValidationTextBlock.Text = "Please enter a valid time. Example: 23:59";
            return false;
        }

        DateTime localDeadline = DateTime.SpecifyKind(
            DeadlineDatePicker.SelectedDate.Value.Date + deadlineTime,
            DateTimeKind.Local
        );

        deadline = new DeadlineItem
        {
            Title = TitleTextBox.Text.Trim(),
            Category = CategoryTextBox.Text.Trim(),
            Description = DescriptionTextBox.Text.Trim(),
            DeadlineAt = new DateTimeOffset(localDeadline),
            CreatedAt = DateTimeOffset.Now,
            IsCompleted = false
        };

        return true;
    }
}
