using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests.WorldClock;

public sealed class ZonePickerViewModelTests
{
    private static readonly TimeZoneInfo SpringFallZone = TestZones.CreateSpringFallZone();
    private static readonly TimeZoneInfo FixedFiveZone = TestZones.CreateFixedFiveZone();

    private static ZonePickerViewModel CreatePicker()
        => new(new ClockZoneFactory(new FakeZoneResolver(SpringFallZone, SpringFallZone, FixedFiveZone)));

    [Fact]
    public void Ctor_LoadsAllZones()
    {
        var picker = CreatePicker();

        Assert.Contains(picker.Zones, z => z.TimeZoneId == "SpringFallTest");
        Assert.Contains(picker.Zones, z => z.TimeZoneId == "FixedFive");
    }

    [Fact]
    public void SearchText_FiltersByLabel_Or_Id()
    {
        var picker = CreatePicker();
        picker.SearchText = "Fixed";

        Assert.Contains(picker.Zones, z => z.TimeZoneId == "FixedFive");
        Assert.DoesNotContain(picker.Zones, z => z.TimeZoneId == "SpringFallTest");

        picker.SearchText = "nope-not-here";
        Assert.Empty(picker.Zones);
    }

    [Fact]
    public void SearchText_FiltersCaseInsensitively()
    {
        var picker = CreatePicker();
        picker.SearchText = "springfall";

        Assert.Contains(picker.Zones, z => z.TimeZoneId == "SpringFallTest");
    }

    [Fact]
    public void Exclude_HidesZone_Include_RestoresIt()
    {
        var picker = CreatePicker();

        picker.Exclude("SpringFallTest");
        Assert.DoesNotContain(picker.Zones, z => z.TimeZoneId == "SpringFallTest");

        picker.Include("SpringFallTest");
        Assert.Contains(picker.Zones, z => z.TimeZoneId == "SpringFallTest");
    }

    [Fact]
    public void SearchText_RaisesZonesPropertyChanged()
    {
        var picker = CreatePicker();
        var changed = new List<string?>();
        picker.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        picker.SearchText = "Fixed";

        Assert.Contains(nameof(ZonePickerViewModel.Zones), changed);
    }

    [Fact]
    public void Exclude_Include_RaiseZonesPropertyChanged()
    {
        var picker = CreatePicker();
        var changed = new List<string?>();
        picker.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        picker.Exclude("SpringFallTest");
        picker.Include("SpringFallTest");

        Assert.Equal(2, changed.Count(name => name == nameof(ZonePickerViewModel.Zones)));
    }
}