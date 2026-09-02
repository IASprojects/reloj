using ChronosFlip.Core.WorldClock;

namespace ChronosFlip.Tests.WorldClock;

public sealed class ClockZoneTests
{
    private static readonly TimeZoneInfo SpringFallZone = TestZones.CreateSpringFallZone();

    private static readonly DateTimeOffset SatStandard =
        new(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset SatDaylight =
        new(2026, 7, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TimeZoneConverter_StandardTime_UsesBaseOffset()
    {
        var local = TimeZoneConverter.ToZoneTime(SatStandard, SpringFallZone);

        Assert.Equal(new DateTimeOffset(2026, 1, 15, 1, 0, 0, TimeSpan.FromHours(1)), local);
    }

    [Fact]
    public void TimeZoneConverter_DaylightTime_AppliesDstOffset()
    {
        var local = TimeZoneConverter.ToZoneTime(SatDaylight, SpringFallZone);

        Assert.Equal(new DateTimeOffset(2026, 7, 15, 2, 0, 0, TimeSpan.FromHours(2)), local);
    }

    [Theory]
    [InlineData("2026-04-05T00:59:00Z", 1, "01:59")]
    [InlineData("2026-04-05T02:00:00Z", 2, "04:00")]
    public void TimeZoneConverter_SpringForward_DstKicksInAtBoundary(string isoInstant, int expectedOffsetHours, string expectedTime)
    {
        var instant = DateTimeOffset.Parse(isoInstant);
        var local = TimeZoneConverter.ToZoneTime(instant, SpringFallZone);

        Assert.Equal(TimeSpan.FromHours(expectedOffsetHours), local.Offset);
        Assert.Equal(expectedTime, local.ToString("HH:mm"));
    }

    [Theory]
    [InlineData("2026-10-25T00:00:00Z", 2, "02:00")]
    [InlineData("2026-10-25T02:00:00Z", 1, "03:00")]
    public void TimeZoneConverter_FallBack_DstEndsAtBoundary(string isoInstant, int expectedOffsetHours, string expectedTime)
    {
        var instant = DateTimeOffset.Parse(isoInstant);
        var local = TimeZoneConverter.ToZoneTime(instant, SpringFallZone);

        Assert.Equal(TimeSpan.FromHours(expectedOffsetHours), local.Offset);
        Assert.Equal(expectedTime, local.ToString("HH:mm"));
    }

    [Fact]
    public void TimeZoneConverter_AmbiguousOverlap_ResolvesDeterministically()
    {
        var instant = new DateTimeOffset(2026, 10, 25, 1, 30, 0, TimeSpan.Zero);
        var local = TimeZoneConverter.ToZoneTime(instant, SpringFallZone);

        Assert.Equal(TimeSpan.FromHours(1), local.Offset);
        Assert.Equal("02:30", local.ToString("HH:mm"));
    }

    [Fact]
    public void Factory_TryCreate_ValidId_WithNullLabel_UsesDefaultDisplayName()
    {
        var factory = new ClockZoneFactory(CreateResolver());

        var ok = factory.TryCreate("SpringFallTest", null, out var zone);

        Assert.True(ok);
        Assert.NotNull(zone);
        Assert.Equal("SpringFallTest", zone.TimeZoneId);
        Assert.Equal("Test Zone", zone.Label);
    }

    [Fact]
    public void Factory_TryCreate_ValidId_WithExplicitLabel_KeepsLabel()
    {
        var factory = new ClockZoneFactory(CreateResolver());

        var ok = factory.TryCreate("SpringFallTest", "My Label", out var zone);

        Assert.True(ok);
        Assert.NotNull(zone);
        Assert.Equal("My Label", zone.Label);
    }

    [Fact]
    public void Factory_TryCreate_UnknownId_ReturnsFalse()
    {
        var factory = new ClockZoneFactory(CreateResolver());

        var ok = factory.TryCreate("No/Such/Zone", null, out var zone);

        Assert.False(ok);
        Assert.Null(zone);
    }

    [Fact]
    public void Factory_AllAvailable_IncludesTestZone()
    {
        var factory = new ClockZoneFactory(CreateResolver());

        var zones = factory.AllAvailable();

        Assert.Contains(zones, z => z.TimeZoneId == "SpringFallTest");
    }

    [Fact]
    public void SystemResolver_UnknownId_ReturnsNull()
    {
        var resolver = new SystemZoneResolver();

        Assert.Null(resolver.Resolve("No/Such/Zone"));
    }

    [Fact]
    public void SystemResolver_LocalId_Resolves()
    {
        var resolver = new SystemZoneResolver();

        Assert.NotNull(resolver.Resolve(TimeZoneInfo.Local.Id));
    }

    [Fact]
    public void SystemResolver_Local_EqualsTimeZoneInfoLocal()
    {
        var resolver = new SystemZoneResolver();

        Assert.Equal(TimeZoneInfo.Local.Id, resolver.Local.Id);
    }

    [Fact]
    public void SystemResolver_AvailableZones_NonEmpty()
    {
        var resolver = new SystemZoneResolver();

        Assert.NotEmpty(resolver.AvailableZones);
    }

    private static IZoneResolver CreateResolver()
    {
        return new FakeZoneResolver(SpringFallZone, SpringFallZone);
    }
}
