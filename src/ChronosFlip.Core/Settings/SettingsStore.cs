using System.Text.Json;
using ChronosFlip.Core.Alarms;

namespace ChronosFlip.Core.Settings;

public sealed class SettingsStore
{
    public const string FileName = "settings.json";

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _filePath;

    public SettingsStore(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Settings directory must be provided.", nameof(directory));
        }

        Directory = directory;
        _filePath = Path.Combine(directory, FileName);
    }

    public string Directory { get; }

    public string FilePath => _filePath;

    public static string DefaultDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "ChronosFlip");
    }

    public ChronosFlipSettings Load()
    {
        EnsureDirectory();

        if (!File.Exists(_filePath))
        {
            return Sanitize(new ChronosFlipSettings());
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            var loaded = JsonSerializer.Deserialize<ChronosFlipSettings>(stream, s_jsonOptions);
            return Sanitize(loaded ?? new ChronosFlipSettings());
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            QuarantineCorruptFile();
            return Sanitize(new ChronosFlipSettings());
        }
    }

    public void Save(ChronosFlipSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        EnsureDirectory();

        var tempPath = _filePath + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            JsonSerializer.Serialize(stream, settings, s_jsonOptions);
            stream.Flush(flushToDisk: true);
        }

        File.Move(tempPath, _filePath, overwrite: true);
    }

    private void EnsureDirectory()
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.CreateDirectory(Directory);
        }
    }

    private void QuarantineCorruptFile()
    {
        try
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var quarantinePath = _filePath + ".corrupt-" + stamp;
            File.Move(_filePath, quarantinePath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static ChronosFlipSettings Sanitize(ChronosFlipSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.NeonHexColor))
        {
            settings.NeonHexColor = SettingsDefaults.NeonHexColor;
        }

        settings.Zones = settings.Zones?
            .Where(zone => zone is not null &&
                           !string.IsNullOrWhiteSpace(zone.TimeZoneId) &&
                           !string.IsNullOrWhiteSpace(zone.Label))
            .GroupBy(zone => zone.TimeZoneId!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList() ?? [];

        settings.Alarms = settings.Alarms?
            .Where(alarm => alarm is not null &&
                            !string.IsNullOrWhiteSpace(alarm.Id) &&
                            !string.IsNullOrWhiteSpace(alarm.ZoneId) &&
                            DateTimeOffset.TryParse(alarm.FireAtUtc, out _))
            .GroupBy(alarm => alarm!.Id!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList() ?? [];

        return settings;
    }
}
