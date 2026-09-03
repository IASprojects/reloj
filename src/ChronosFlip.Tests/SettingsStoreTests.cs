using ChronosFlip.Core.Alarms;
using ChronosFlip.Core.Settings;
using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _directory;

    public SettingsStoreTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "ChronosFlip.Tests", Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileMissing()
    {
        var store = new SettingsStore(_directory);

        var loaded = store.Load();

        Assert.NotNull(loaded);
        Assert.False(loaded.NeonEnabled);
        Assert.Equal(SettingsDefaults.NeonHexColor, loaded.NeonHexColor);
        Assert.False(loaded.PinToTop);
        Assert.Null(loaded.Window);
        Assert.NotNull(loaded.Zones);
        Assert.Empty(loaded.Zones);
    }

    [Fact]
    public void Save_Then_Load_RoundTripsAllFields()
    {
        var store = new SettingsStore(_directory);
        var input = new ChronosFlipSettings
        {
            NeonEnabled = true,
            NeonHexColor = "#FF00AA",
            PinToTop = true,
            Window = new WindowBounds { X = 100, Y = 80, Width = 720, Height = 360 },
        };

        store.Save(input);
        var loaded = store.Load();

        Assert.True(loaded.NeonEnabled);
        Assert.Equal("#FF00AA", loaded.NeonHexColor);
        Assert.True(loaded.PinToTop);
        Assert.NotNull(loaded.Window);
        Assert.Equal(100, loaded.Window!.X);
        Assert.Equal(80, loaded.Window.Y);
        Assert.Equal(720, loaded.Window.Width);
        Assert.Equal(360, loaded.Window.Height);
    }

    [Fact]
    public void Save_DoesNotLeaveTempFile()
    {
        var store = new SettingsStore(_directory);
        store.Save(new ChronosFlipSettings());

        Assert.True(File.Exists(store.FilePath));
        Assert.False(File.Exists(store.FilePath + ".tmp"));
    }

    [Fact]
    public void Save_CreatesDirectoryIfMissing()
    {
        Assert.False(Directory.Exists(_directory));
        var store = new SettingsStore(_directory);

        store.Save(new ChronosFlipSettings());

        Assert.True(Directory.Exists(_directory));
        Assert.True(File.Exists(store.FilePath));
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileIsCorruptJson()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, "{ this is not valid json ");

        var loaded = store.Load();

        Assert.NotNull(loaded);
        Assert.Equal(SettingsDefaults.NeonHexColor, loaded.NeonHexColor);
        Assert.False(loaded.NeonEnabled);
        Assert.False(File.Exists(store.FilePath));
        Assert.Contains(Directory.GetFiles(_directory), p => p.Contains(".corrupt-", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_ReplacesBlankNeonColorWithDefault()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, "{\"NeonHexColor\":\"   \",\"NeonEnabled\":true}");

        var loaded = store.Load();

        Assert.Equal(SettingsDefaults.NeonHexColor, loaded.NeonHexColor);
        Assert.True(loaded.NeonEnabled);
    }

    [Fact]
    public void SecondStore_ReadsFirstStoresWrites()
    {
        var first = new SettingsStore(_directory);
        first.Save(new ChronosFlipSettings { NeonEnabled = true, NeonHexColor = "#123456" });

        var second = new SettingsStore(_directory);
        var loaded = second.Load();

        Assert.True(loaded.NeonEnabled);
        Assert.Equal("#123456", loaded.NeonHexColor);
    }

    [Fact]
    public void Save_Then_Load_RoundTripsZones()
    {
        var store = new SettingsStore(_directory);
        var input = new ChronosFlipSettings
        {
            Zones = new List<ClockZoneRef>
            {
                new() { Label = "Paris", TimeZoneId = "Romance Standard Time" },
                new() { Label = "Tokyo", TimeZoneId = "Tokyo Standard Time" },
            },
        };

        store.Save(input);
        var loaded = store.Load();

        Assert.NotNull(loaded.Zones);
        Assert.Equal(2, loaded.Zones.Count);
        Assert.Equal("Paris", loaded.Zones[0].Label);
        Assert.Equal("Romance Standard Time", loaded.Zones[0].TimeZoneId);
        Assert.Equal("Tokyo", loaded.Zones[1].Label);
        Assert.Equal("Tokyo Standard Time", loaded.Zones[1].TimeZoneId);
    }

    [Fact]
    public void Load_NormalizesMissingZones_ToEmptyList()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, "{\"NeonEnabled\":true}");

        var loaded = store.Load();

        Assert.NotNull(loaded.Zones);
        Assert.Empty(loaded.Zones);
    }

    [Fact]
    public void Load_DeduplicatesZones_ByTimeZoneId()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, """
        {
          "Zones": [
            { "Label": "A", "TimeZoneId": "Tokyo Standard Time" },
            { "Label": "B", "TimeZoneId": "Tokyo Standard Time" },
            { "Label": "", "TimeZoneId": "Romance Standard Time" },
            { "Label": "No ID", "TimeZoneId": "  " }
          ]
        }
        """);

        var loaded = store.Load();

        Assert.NotNull(loaded.Zones);
        Assert.Single(loaded.Zones!, z => z.TimeZoneId == "Tokyo Standard Time");
        Assert.Equal("A", loaded.Zones!.Single(z => z.TimeZoneId == "Tokyo Standard Time").Label);
        Assert.DoesNotContain(loaded.Zones!, z => string.IsNullOrWhiteSpace(z.Label));
        Assert.DoesNotContain(loaded.Zones!, z => string.IsNullOrWhiteSpace(z.TimeZoneId));
    }

    [Fact]
    public void Load_DeduplicatesZones_CaseInsensitive()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, """
        {
          "Zones": [
            { "Label": "A", "TimeZoneId": "tokyo standard time" },
            { "Label": "B", "TimeZoneId": "Tokyo Standard Time" }
          ]
        }
        """);

        var loaded = store.Load();

        Assert.NotNull(loaded.Zones);
        Assert.Single(loaded.Zones);
        Assert.Equal("A", loaded.Zones[0].Label);
    }

    [Fact]
    public void Save_Then_Load_RoundTripsAlarms()
    {
        var store = new SettingsStore(_directory);
        var input = new ChronosFlipSettings();
        input.Alarms = new List<AlarmRef>
        {
            AlarmRef.FromAlarm(Alarm.Restore("a1", "Tokyo Standard Time",
                new DateTimeOffset(2026, 9, 12, 2, 30, 0, TimeSpan.Zero), "Tokyo")),
            AlarmRef.FromAlarm(Alarm.Restore("a2", "Paris",
                new DateTimeOffset(2026, 9, 13, 7, 0, 0, TimeSpan.Zero))),
        };
        input.Alarms[1].IsEnabled = false;

        store.Save(input);
        var loaded = store.Load();

        Assert.NotNull(loaded.Alarms);
        Assert.Equal(2, loaded.Alarms!.Count);
        Assert.Equal("a1", loaded.Alarms[0].Id);
        Assert.Equal("Tokyo Standard Time", loaded.Alarms[0].ZoneId);
        Assert.True(loaded.Alarms[0].IsEnabled);
        Assert.False(loaded.Alarms[1].IsEnabled);
        Assert.StartsWith("2026-09-12", loaded.Alarms[0].FireAtUtc!, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_NormalizesMissingAlarms_ToEmptyList()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, "{\"NeonEnabled\":true}");

        var loaded = store.Load();

        Assert.NotNull(loaded.Alarms);
        Assert.Empty(loaded.Alarms!);
    }

    [Fact]
    public void Load_DeduplicatesAlarms_ById()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, """
        {
          "Alarms": [
            { "Id": "a1", "ZoneId": "Tokyo Standard Time", "FireAtUtc": "2026-09-12T02:30:00Z", "IsEnabled": true },
            { "Id": "a1", "ZoneId": "Paris", "FireAtUtc": "2026-09-13T07:00:00Z", "IsEnabled": true },
            { "Id": "", "ZoneId": "Paris", "FireAtUtc": "2026-09-13T07:00:00Z", "IsEnabled": true },
            { "Id": "a3", "ZoneId": "Paris", "FireAtUtc": "not-a-date", "IsEnabled": true },
            { "Id": "a4", "ZoneId": "  ", "FireAtUtc": "2026-09-13T07:00:00Z", "IsEnabled": true }
          ]
        }
        """);

        var loaded = store.Load();

        Assert.NotNull(loaded.Alarms);
        Assert.Single(loaded.Alarms!);
        Assert.Equal("a1", loaded.Alarms[0].Id);
        Assert.Equal("Tokyo Standard Time", loaded.Alarms[0].ZoneId);
    }

    [Fact]
    public void Load_DeduplicatesAlarms_CaseInsensitive()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, """
        {
          "Alarms": [
            { "Id": "A1", "ZoneId": "Tokyo Standard Time", "FireAtUtc": "2026-09-12T02:30:00Z", "IsEnabled": true },
            { "Id": "a1", "ZoneId": "Paris", "FireAtUtc": "2026-09-13T07:00:00Z", "IsEnabled": true }
          ]
        }
        """);

        var loaded = store.Load();

        Assert.NotNull(loaded.Alarms);
        Assert.Single(loaded.Alarms!);
    }

    [Fact]
    public void Load_DefaultsTimerPreset_WhenMissing()
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, "{\"NeonEnabled\":true}");

        var loaded = store.Load();

        Assert.Equal(SettingsDefaults.TimerPresetSeconds, loaded.TimerPresetSeconds);
    }

    [Fact]
    public void Save_Then_Load_RoundTripsTimerPreset()
    {
        var store = new SettingsStore(_directory);
        store.Save(new ChronosFlipSettings { TimerPresetSeconds = 90 });

        var loaded = store.Load();

        Assert.Equal(90, loaded.TimerPresetSeconds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(6000)]
    public void Load_RejectsOutOfRangeTimerPreset_WithDefault(int value)
    {
        var store = new SettingsStore(_directory);
        Directory.CreateDirectory(_directory);
        File.WriteAllText(store.FilePath, $"{{\"TimerPresetSeconds\":{value}}}");

        var loaded = store.Load();

        Assert.Equal(SettingsDefaults.TimerPresetSeconds, loaded.TimerPresetSeconds);
    }
}