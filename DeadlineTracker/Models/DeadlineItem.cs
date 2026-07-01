using DeadlineTracker.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DeadlineTracker.Models;

public class DeadlineItem : INotifyPropertyChanged
{
    private Guid _id = Guid.NewGuid();
    private string _title = string.Empty;
    private string _category = string.Empty;
    private string _description = string.Empty;
    private DateTimeOffset _deadlineAt;
    private bool _isCompleted;
    private DateTimeOffset _createdAt = DateTimeOffset.Now;
    private DateTimeOffset? _completedAt;

    public Guid Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public DateTimeOffset DeadlineAt
    {
        get => _deadlineAt;
        set
        {
            if (!SetField(ref _deadlineAt, value))
                return;

            RefreshTimeDependentProperties();
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (!SetField(ref _isCompleted, value))
                return;

            RefreshTimeDependentProperties();
        }
    }

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        set => SetField(ref _createdAt, value);
    }

    public DateTimeOffset? CompletedAt
    {
        get => _completedAt;
        set => SetField(ref _completedAt, value);
    }

    [JsonIgnore]
    public string RemainingTimeText
    {
        get
        {
            if (IsCompleted)
                return "Completed";

            return DeadlineTimeFormatter.FormatRemainingTime(DeadlineAt, DateTimeOffset.Now);
        }
    }

    [JsonIgnore]
    public string StatusText
    {
        get
        {
            if (IsCompleted)
                return "Completed";

            return IsOverdue ? "Overdue" : "Active";
        }
    }

    [JsonIgnore]
    public bool IsOverdue => !IsCompleted && DeadlineAt <= DateTimeOffset.Now;

    public void RefreshTimeDependentProperties()
    {
        OnPropertyChanged(nameof(RemainingTimeText));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(IsOverdue));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
