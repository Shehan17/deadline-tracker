using DeadlineTracker.Models;
using System.IO;
using System.Text.Json;

namespace DeadlineTracker.Services;

public class DeadlineStorageService : IDeadlineStorageService
{
    private readonly string _appFolder;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public DeadlineStorageService()
    {
        _appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeadlineTracker"
        );

        _filePath = Path.Combine(_appFolder, "deadlines.json");
    }

    public async Task<List<DeadlineItem>> LoadDeadlinesAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<DeadlineItem>();
        }

        try
        {
            string json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<DeadlineItem>();
            }

            List<DeadlineItem>? deadlines = JsonSerializer.Deserialize<List<DeadlineItem>>(json);

            if (deadlines is null)
            {
                return new List<DeadlineItem>();
            }

            ApplyLegacyDeadlineValues(json, deadlines);

            return deadlines;
        }
        catch
        {
            return new List<DeadlineItem>();
        }
    }

    public async Task SaveDeadlinesAsync(IEnumerable<DeadlineItem> deadlines)
    {
        try
        {
            Directory.CreateDirectory(_appFolder);

            string json = JsonSerializer.Serialize(deadlines, _jsonOptions);

            await File.WriteAllTextAsync(_filePath, json);
        }
        catch
        {
        }
    }

    private static void ApplyLegacyDeadlineValues(string json, List<DeadlineItem> deadlines)
    {
        using JsonDocument document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return;

        int deadlineCount = Math.Min(deadlines.Count, document.RootElement.GetArrayLength());

        for (int i = 0; i < deadlineCount; i++)
        {
            DeadlineItem deadline = deadlines[i];

            if (deadline.DeadlineAt != default)
                continue;

            JsonElement item = document.RootElement[i];

            if (!item.TryGetProperty("Deadline", out JsonElement legacyDeadline))
                continue;

            if (legacyDeadline.TryGetDateTimeOffset(out DateTimeOffset deadlineAt))
            {
                deadline.DeadlineAt = deadlineAt;
            }
            else if (legacyDeadline.TryGetDateTime(out DateTime localDeadline))
            {
                deadline.DeadlineAt = new DateTimeOffset(DateTime.SpecifyKind(localDeadline, DateTimeKind.Local));
            }
        }
    }
}
