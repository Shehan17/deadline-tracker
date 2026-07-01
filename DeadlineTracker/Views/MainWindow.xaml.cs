using DeadlineTracker.Models;
using DeadlineTracker.Services;
using DeadlineTracker.ViewModels;
using System.Windows;

namespace DeadlineTracker.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel(
            new DeadlineStorageService(),
            ShowAddDeadlineDialog,
            ConfirmDeleteDeadline
        );
        DataContext = _viewModel;

        Closed += MainWindow_Closed;
    }

    private DeadlineItem? ShowAddDeadlineDialog()
    {
        AddDeadlineDialog dialog = new()
        {
            Owner = this
        };

        return dialog.ShowDialog() == true ? dialog.CreatedDeadline : null;
    }

    private static bool ConfirmDeleteDeadline(DeadlineItem deadline)
    {
        MessageBoxResult result = MessageBox.Show(
            $"Delete \"{deadline.Title}\"?",
            "Delete Deadline",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        return result == MessageBoxResult.Yes;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _viewModel.Dispose();
    }
}
