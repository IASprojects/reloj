using ChronosFlip.Core.Clocks;
using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests.WorldClock;

public sealed class WorldClockViewModelTests
{
    private static readonly TimeZoneInfo SpringFallZone = TestZones.CreateSpringFallZone();
    private static readonly TimeZoneInfo FixedFiveZone = TestZones.CreateFixedFiveZone();
    private static readonly TimeZoneInfo FixedThreeZone = TestZones.CreateFixedThreeZone();

    private static FakeZoneResolver CreateResolver()
        => new(SpringFallZone, SpringFallZone, FixedFiveZone, FixedThreeZone);

    private static FakeZoneResolver CreateResolver(TimeZoneInfo local, params TimeZoneInfo[] zones)
        => new(local, zones);

    [Fact]
    public void Ctor_AddsLocalCardFirst()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);

        Assert.NotEmpty(viewModel.Cards);
        Assert.Equal("SpringFallTest", viewModel.Cards[0].TimeZoneId);
        Assert.Same(viewModel.LocalCard, viewModel.Cards[0]);
        Assert.False(viewModel.Cards[0].IsRemovable);
    }

    [Fact]
    public void Ctor_AddsProvidedZonesInOrder_AfterLocalCard()
    {
        var zones = new[]
        {
            new ClockZone { Label = "Five", TimeZoneId = "FixedFive" },
            new ClockZone { Label = "Three", TimeZoneId = "FixedThree" },
        };

        var viewModel = new WorldClockViewModel(CreateResolver(), zones);

        Assert.Equal(3, viewModel.Cards.Count);
        Assert.Equal("FixedFive", viewModel.Cards[1].TimeZoneId);
        Assert.Equal("FixedThree", viewModel.Cards[2].TimeZoneId);
    }

    [Fact]
    public void Ctor_SkipsZones_ThatNoLongerResolve()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), new[]
        {
            new ClockZone { Label = "Ghost", TimeZoneId = "No/Such/Zone" },
            new ClockZone { Label = "Five", TimeZoneId = "FixedFive" },
        });

        Assert.Equal(2, viewModel.Cards.Count);
        Assert.DoesNotContain(viewModel.Cards, c => c.TimeZoneId == "No/Such/Zone");
    }

    [Fact]
    public void ZonesToPersist_ReturnsNonLocalZones_InOrder()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        viewModel.AddZone(new ClockZone { Label = "Five", TimeZoneId = "FixedFive" });
        viewModel.AddZone(new ClockZone { Label = "Three", TimeZoneId = "FixedThree" });

        var zones = viewModel.ZonesToPersist();

        Assert.Equal(2, zones.Count);
        Assert.Equal("FixedFive", zones[0].TimeZoneId);
        Assert.Equal("FixedThree", zones[1].TimeZoneId);
        Assert.DoesNotContain(zones, z => z.TimeZoneId == "SpringFallTest");
    }

    [Fact]
    public void ZonesToPersist_ReturnsEmpty_WhenOnlyLocalCard()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);

        Assert.Empty(viewModel.ZonesToPersist());
    }

    [Fact]
    public void AddZone_AddsResolvedZoneCard()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        var zone = new ClockZone { Label = "My Label", TimeZoneId = "FixedFive" };

        var added = viewModel.AddZone(zone);

        Assert.True(added);
        Assert.Equal(2, viewModel.Cards.Count);
        Assert.Equal("My Label", viewModel.Cards[1].Label);
        Assert.Equal("FixedFive", viewModel.Cards[1].TimeZoneId);
        Assert.True(viewModel.Cards[1].IsRemovable);
    }

    [Fact]
    public void AddZone_DuplicateTimeZoneId_ReturnsFalse()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        var zone = new ClockZone { Label = "A", TimeZoneId = "FixedFive" };
        viewModel.AddZone(zone);

        var again = viewModel.AddZone(zone);

        Assert.False(again);
        Assert.Equal(2, viewModel.Cards.Count);
    }

    [Fact]
    public void AddZone_UnknownId_ReturnsFalse_AndAddsNothing()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        var zone = new ClockZone { Label = "Ghost", TimeZoneId = "No/Such/Zone" };

        var added = viewModel.AddZone(zone);

        Assert.False(added);
        Assert.Single(viewModel.Cards);
    }

    [Fact]
    public void AddZone_LocalZoneId_ReturnsFalse()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        var zone = new ClockZone { Label = "Local", TimeZoneId = "SpringFallTest" };

        var added = viewModel.AddZone(zone);

        Assert.False(added);
        Assert.Single(viewModel.Cards);
    }

    [Fact]
    public void RemoveZone_RemovesNonLocalCard()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        viewModel.AddZone(new ClockZone { Label = "Five", TimeZoneId = "FixedFive" });

        var removed = viewModel.RemoveZone("FixedFive");

        Assert.True(removed);
        Assert.DoesNotContain(viewModel.Cards, c => c.TimeZoneId == "FixedFive");
        Assert.Contains(viewModel.Cards, c => c.TimeZoneId == "SpringFallTest");
    }

    [Fact]
    public void RemoveZone_LocalId_ReturnsFalse()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);

        var removed = viewModel.RemoveZone("SpringFallTest");

        Assert.False(removed);
        Assert.Single(viewModel.Cards);
    }

    [Fact]
    public void RemoveZone_UnknownId_ReturnsFalse()
    {
        var viewModel = new WorldClockViewModel(CreateResolver(), []);

        var removed = viewModel.RemoveZone("No/Such/Zone");

        Assert.False(removed);
    }

    [Fact]
    public void SinglePump_AdvancesAllCardsInLockstep_FromSameInstant()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero));
        var ticker = new ClockTicker(clock);
        var viewModel = new WorldClockViewModel(CreateResolver(), []);
        viewModel.AddZone(new ClockZone { Label = "Five", TimeZoneId = "FixedFive" });
        viewModel.Attach(ticker);

        clock.Now = new DateTimeOffset(2026, 7, 15, 0, 0, 42, TimeSpan.Zero);
        ticker.Pump();

        Assert.All(viewModel.Cards, card => Assert.Equal("42", card.Seconds));
        Assert.Equal("02", viewModel.Cards[0].Hours);
        Assert.Equal("UTC+02:00", viewModel.Cards[0].OffsetText);
        Assert.Equal("05", viewModel.Cards[1].Hours);
        Assert.Equal("UTC+05:00", viewModel.Cards[1].OffsetText);
    }

    [Fact]
    public void Attach_AppliesCurrentInstant_Immediately()
    {
        var clock = new FakeClock(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero));
        var ticker = new ClockTicker(clock);
        var viewModel = new WorldClockViewModel(CreateResolver(), []);

        viewModel.Attach(ticker);

        Assert.Equal("01", viewModel.LocalCard.Hours);
        Assert.Equal("UTC+01:00", viewModel.LocalCard.OffsetText);
    }
}