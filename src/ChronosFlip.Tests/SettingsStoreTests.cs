using ChronosFlip.Core.Settings;

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
}
