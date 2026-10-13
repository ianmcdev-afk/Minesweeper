using System.IO;
using System.Text.Json;

namespace Minesweeper;

public sealed class ScoreEntry
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public long Ms { get; set; }
    public DateTime Date { get; set; }
}

public sealed class ScoreData
{
    public string LastName { get; set; } = "";
    public string Theme { get; set; } = "";
    public Dictionary<string, List<ScoreEntry>> Scores { get; set; } = new();
}

/// <summary>
/// Reads and writes %LocalAppData%\Minesweeper\scores.json, so scores survive
/// moving, rebuilding or re-extracting the exe.
/// </summary>
public static class ScoreStore
{
    public const int Max = 10;

    static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string FilePath { get; } = ResolvePath();

    static string ResolvePath()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Minesweeper");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "scores.json");
    }

    public static ScoreData Load()
    {
        if (!File.Exists(FilePath)) return new ScoreData();
        try
        {
            var data = JsonSerializer.Deserialize<ScoreData>(File.ReadAllText(FilePath), Options) ?? new ScoreData();
            foreach (var key in data.Scores.Keys.ToList())
                data.Scores[key] = data.Scores[key].OrderBy(e => e.Ms).ThenBy(e => e.Date).Take(Max).ToList();
            return data;
        }
        catch
        {
            // Unreadable file: keep a copy rather than silently overwriting it.
            try { File.Move(FilePath, FilePath + ".bad", true); } catch { }
            return new ScoreData();
        }
    }

    public static bool Save(ScoreData data)
    {
        try
        {
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data, Options));
            File.Move(tmp, FilePath, true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
