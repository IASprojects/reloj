using ChronosFlip.Core.Settings;
using ChronosFlip.Core.ViewModels;
using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests;

public sealed class SettingsViewModelTests : IDisposable
{
    private readonly string _directory;

    public SettingsViewModelTests()
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
    public void Load_PopulatesProperties_FromStore()
    {
        var store = new SettingsStore(_directory);
        store.Save(new ChronosFlipSettings { NeonEnabled = true, NeonHexColor = "#AABBCC" });
        var vm = new SettingsViewModel(store);

        vm.Load();

        Assert.True(vm.NeonEnabled);
        Assert.Equal("#AABBCC", vm.NeonHexColor);
    }

    [Fact]
    public void Load_AppliesDefaults_WhenStoreHasNoFile()
    {
        var store = new SettingsStore(_directory);
        var vm = new SettingsViewModel(store);

        vm.Load();

        Assert.False(vm.NeonEnabled);
        Assert.Equal(SettingsDefaults.NeonHexColor, vm.NeonHexColor);
    }

    [Fact]
    public void Save_PersistsCurrentPropertiesToStore()
    {
        var store = new SettingsStore(_directory);
        var vm = new SettingsViewModel(store)
        {
            NeonEnabled = true,
            NeonHexColor = "#0F0F0F",
            PinToTop = true,
        };

        vm.Save();
        var reloaded = store.Load();

        Assert.True(reloaded.NeonEnabled);
        Assert.Equal("#0F0F0F", reloaded.NeonHexColor);
        Assert.True(reloaded.PinToTop);
    }

    [Fact]
    public void Setting_NeonEnabled_RaisesPropertyChanged()
    {
        var store = new SettingsStore(_directory);
        var vm = new SettingsViewModel(store);
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.NeonEnabled = true;

        Assert.Contains(nameof(SettingsViewModel.NeonEnabled), raised);
    }

    [Fact]
    public void Load_PopulatesZones_FromStore()
    {
        var store = new SettingsStore(_directory);
        store.Save(new ChronosFlipSettings
        {
            Zones = new List<ClockZoneRef>
            {
                new() { Label = "Berlin", TimeZoneId = "W. Europe Standard Time" },
            },
        });
        var vm = new SettingsViewModel(store);

        vm.Load();

        Assert.Single(vm.Zones);
        Assert.Equal("Berlin", vm.Zones[0].Label);
        Assert.Equal("W. Europe Standard Time", vm.Zones[0].TimeZoneId);
    }

    [Fact]
    public void SetZones_PersistsOnSave()
    {
        var store = new SettingsStore(_directory);
        var vm = new SettingsViewModel(store);
        vm.Load();
        vm.SetZones(new[]
        {
            new ClockZone { Label = "Nairobi", TimeZoneId = "E. Africa Standard Time" },
        });

        vm.Save();
        var reloaded = store.Load();

        Assert.NotNull(reloaded.Zones);
        Assert.Single(reloaded.Zones);
        Assert.Equal("Nairobi", reloaded.Zones![0].Label);
        Assert.Equal("E. Africa Standard Time", reloaded.Zones[0].TimeZoneId);
    }

    [Fact]
    public void Apply_TreatsNullZonesAsEmpty()
    {
        var vm = new SettingsViewModel(new SettingsStore(_directory));

        vm.Apply(new ChronosFlipSettings { Zones = null });

        Assert.Empty(vm.Zones);
    }

    [Fact]
    public void Apply_DropsNullEntriesInZoneList()
    {
        var vm = new SettingsViewModel(new SettingsStore(_directory));

        vm.Apply(new ChronosFlipSettings
        {
            Zones = new List<ClockZoneRef?>
            {
                null,
                new() { Label = "Madrid", TimeZoneId = "Romance Standard Time" },
                new() { Label = null, TimeZoneId = null },
            },
        });

        Assert.Single(vm.Zones);
        Assert.Equal("Madrid", vm.Zones[0].Label);
    }
}
