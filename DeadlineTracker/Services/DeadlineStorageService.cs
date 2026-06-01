using DeadlineTracker.Models;
using System.IO;
using System.Text.Json;


namespace DeadlineTracker.Services;

public class DeadlineStorageService
{
    private readonly string filePath;

    public DeadlineStorageService()
    {
        string appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeadlineTracker"
        );

        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        filePath = Path.Combine(appFolder, "deadlines.json");
    }

    public List<DeadlineItem> LoadDeadlines()
    {
        if (!File.Exists(filePath))
        {
            return new List<DeadlineItem>();
        }

        try
        {
            string json = File.ReadAllText(filePath);

            List<DeadlineItem>? deadlines = JsonSerializer.Deserialize<List<DeadlineItem>>(json);

            return deadlines ?? new List<DeadlineItem>();
        }
        catch
        {
            return new List<DeadlineItem>();
        }
    }

    public void SaveDeadlines(List<DeadlineItem> deadlines)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(deadlines, options);

        File.WriteAllText(filePath, json);
    }
}