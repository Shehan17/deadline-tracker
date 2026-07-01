using DeadlineTracker.Models;

namespace DeadlineTracker.Services;

public interface IDeadlineStorageService
{
    Task<List<DeadlineItem>> LoadDeadlinesAsync();

    Task SaveDeadlinesAsync(IEnumerable<DeadlineItem> deadlines);
}
