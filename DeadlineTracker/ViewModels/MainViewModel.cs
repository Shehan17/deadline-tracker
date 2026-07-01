using DeadlineTracker.Commands;
using DeadlineTracker.Models;
using DeadlineTracker.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace DeadlineTracker.ViewModels;

public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IDeadlineStorageService _storageService;
    private readonly Func<DeadlineItem?> _createDeadline;
    private readonly Func<DeadlineItem, bool> _confirmDelete;
    private readonly DispatcherTimer _countdownTimer;
    private DeadlineItem? _selectedDeadline;

    public MainViewModel(
        IDeadlineStorageService storageService,
        Func<DeadlineItem?> createDeadline,
        Func<DeadlineItem, bool> confirmDelete)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _createDeadline = createDeadline ?? throw new ArgumentNullException(nameof(createDeadline));
        _confirmDelete = confirmDelete ?? throw new ArgumentNullException(nameof(confirmDelete));

        Deadlines = new ObservableCollection<DeadlineItem>();
        AddDeadlineCommand = new RelayCommand(AddDeadline);
        DeleteDeadlineCommand = new RelayCommand(DeleteDeadline, CanModifyDeadline);
        ToggleCompleteCommand = new RelayCommand(ToggleComplete, CanModifyDeadline);

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += CountdownTimer_Tick;
        _countdownTimer.Start();

        _ = LoadDeadlinesAsync();
    }

    public ObservableCollection<DeadlineItem> Deadlines { get; }

    public DeadlineItem? SelectedDeadline
    {
        get => _selectedDeadline;
        set
        {
            if (_selectedDeadline == value)
                return;

            _selectedDeadline = value;
            OnPropertyChanged();
            RaiseCommandStatesChanged();
        }
    }

    public ICommand AddDeadlineCommand { get; }

    public ICommand DeleteDeadlineCommand { get; }

    public ICommand ToggleCompleteCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        _countdownTimer.Stop();
        _countdownTimer.Tick -= CountdownTimer_Tick;
    }

    private async void AddDeadline()
    {
        DeadlineItem? deadline = _createDeadline();

        if (deadline is null)
            return;

        if (!IsValidDeadline(deadline))
            return;

        Deadlines.Add(deadline);
        SelectedDeadline = deadline;
        await SaveDeadlinesAsync();
    }

    private async void DeleteDeadline(object? parameter)
    {
        DeadlineItem? deadline = GetDeadline(parameter);

        if (deadline is null)
            return;

        if (!_confirmDelete(deadline))
            return;

        Deadlines.Remove(deadline);

        if (SelectedDeadline == deadline)
        {
            SelectedDeadline = null;
        }

        await SaveDeadlinesAsync();
    }

    private async void ToggleComplete(object? parameter)
    {
        DeadlineItem? deadline = GetDeadline(parameter);

        if (deadline is null)
            return;

        deadline.IsCompleted = !deadline.IsCompleted;
        deadline.CompletedAt = deadline.IsCompleted ? DateTimeOffset.Now : null;
        deadline.RefreshTimeDependentProperties();

        await SaveDeadlinesAsync();
    }

    private async Task LoadDeadlinesAsync()
    {
        List<DeadlineItem> loadedDeadlines = await _storageService.LoadDeadlinesAsync();

        Deadlines.Clear();

        foreach (DeadlineItem deadline in loadedDeadlines.OrderBy(deadline => deadline.DeadlineAt))
        {
            deadline.RefreshTimeDependentProperties();
            Deadlines.Add(deadline);
        }
    }

    private async Task SaveDeadlinesAsync()
    {
        await _storageService.SaveDeadlinesAsync(Deadlines);
    }

    private bool CanModifyDeadline(object? parameter)
    {
        return GetDeadline(parameter) is not null;
    }

    private DeadlineItem? GetDeadline(object? parameter)
    {
        return parameter as DeadlineItem ?? SelectedDeadline;
    }

    private static bool IsValidDeadline(DeadlineItem deadline)
    {
        return !string.IsNullOrWhiteSpace(deadline.Title)
            && deadline.DeadlineAt != default;
    }

    private void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        foreach (DeadlineItem deadline in Deadlines)
        {
            deadline.RefreshTimeDependentProperties();
        }

        RaiseCommandStatesChanged();
    }

    private void RaiseCommandStatesChanged()
    {
        if (DeleteDeadlineCommand is RelayCommand deleteCommand)
        {
            deleteCommand.RaiseCanExecuteChanged();
        }

        if (ToggleCompleteCommand is RelayCommand toggleCommand)
        {
            toggleCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
