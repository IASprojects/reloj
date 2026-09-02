namespace ChronosFlip.Tests.WorldClock;

/// <summary>
/// Deterministic test time zone with a DST rule so world-clock tests are
/// machine-independent.
/// </summary>
internal static class TestZones
{
    /// <summary>
    /// Zone "SpringFallTest": base UTC offset +01:00, daylight delta +1h.
    /// DST starts the first Sunday of April at 02:00 local standard time and
    /// ends the last Sunday of October at 03:00 local daylight time.
    /// Transition instants in UTC (per spec): spring forward 2026-04-05T01:00Z,
    /// fall back 2026-10-25T01:00Z.
    /// </summary>
    private static readonly TimeZoneInfo SpringFallZone = CreateSpringFallZoneCore();

    private static readonly TimeZoneInfo FixedFiveZone = CreateFixedFiveZoneCore();

    private static readonly TimeZoneInfo FixedThreeZone = CreateFixedThreeZoneCore();

    public static TimeZoneInfo CreateSpringFallZone() => SpringFallZone;

    public static TimeZoneInfo CreateFixedFiveZone() => FixedFiveZone;

    public static TimeZoneInfo CreateFixedThreeZone() => FixedThreeZone;

    private static TimeZoneInfo CreateSpringFallZoneCore()
    {
        var transitionStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
            new DateTime(1, 1, 1, 2, 0, 0), month: 4, week: 1, DayOfWeek.Sunday);
        var transitionEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
            new DateTime(1, 1, 1, 3, 0, 0), month: 10, week: 5, DayOfWeek.Sunday);
        var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            new DateTime(2001, 1, 1), DateTime.MaxValue.Date, TimeSpan.FromHours(1),
            transitionStart, transitionEnd);

        return TimeZoneInfo.CreateCustomTimeZone(
            "SpringFallTest",
            TimeSpan.FromHours(1),
            "(UTC+01:00) Test Zone",
            "Test Standard",
            "Test Daylight",
            new[] { rule });
    }

    /// <summary>
    /// Fixed-offset zone "FixedFive" (UTC+05:00, no DST) — a deterministic
    /// second zone for multi-card / offset-formatting tests.
    /// </summary>
    private static TimeZoneInfo CreateFixedFiveZoneCore()
    {
        return TimeZoneInfo.CreateCustomTimeZone(
            "FixedFive",
            TimeSpan.FromHours(5),
            "(UTC+05:00) Fixed Five",
            "Fixed Five");
    }

    /// <summary>
    /// Fixed-offset zone "FixedThree" (UTC+03:00, no DST) — a second
    /// deterministic zone so multi-card ordering tests have a distinct id.
    /// </summary>
    private static TimeZoneInfo CreateFixedThreeZoneCore()
    {
        return TimeZoneInfo.CreateCustomTimeZone(
            "FixedThree",
            TimeSpan.FromHours(3),
            "(UTC+03:00) Fixed Three",
            "Fixed Three");
    }
}