using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Controls;

/// <summary>
/// Compact flip-card for a single world-clock zone: bordered card with the
/// horizontal bisect line, "HH:MM" in Space Mono, then the zone label and its
/// live UTC offset underneath (FR-13). Pure presentation — driven entirely by
/// dependency properties.
/// </summary>
public sealed partial class WorldClockCardControl : UserControl
{
    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register(
            nameof(Time),
            typeof(string),
            typeof(WorldClockCardControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ZoneLabelProperty =
        DependencyProperty.Register(
            nameof(ZoneLabel),
            typeof(string),
            typeof(WorldClockCardControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register(
            nameof(Offset),
            typeof(string),
            typeof(WorldClockCardControl),
            new PropertyMetadata(string.Empty));

    public WorldClockCardControl()
    {
        InitializeComponent();
    }

    public string Time
    {
        get => (string)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public string ZoneLabel
    {
        get => (string)GetValue(ZoneLabelProperty);
        set => SetValue(ZoneLabelProperty, value);
    }

    public string Offset
    {
        get => (string)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }
}