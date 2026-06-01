using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DeadlineTracker.Models
{
    public class DeadlineItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime Deadline { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public string RemainingText
        {
            get
            {
                if (IsCompleted)
                    return "Completed";

                TimeSpan remaining = Deadline - DateTime.Now;

                if (remaining.TotalSeconds <= 0)
                    return "Expired";

                return $"{remaining.Days}d {remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s left";
            }
        }

        [JsonIgnore]
        public string StatusText
        {
            get
            {
                if (IsCompleted)
                    return "Completed";

                if (Deadline <= DateTime.Now)
                    return "Expired";

                return "Active";
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(RemainingText));
            OnPropertyChanged(nameof(StatusText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
