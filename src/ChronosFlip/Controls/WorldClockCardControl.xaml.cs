using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ChronosFlip.Controls;

/// <summary>
/// Compact flip-card for a single world-clock zone: bordered card with the
/// horizontal bisect line, "HH:MM" in Space Mono, then the zone label and its
/// live UTC offset underneath (FR-13). An alarm bell badge shows when the zone
/// has an armed alarm (FR-21); a ringing alarm receives a neon highlight and a
/// STOP affordance (FR-22). Pure presentation — driven entirely by dependency
/// properties.
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

    public static readonly DependencyProperty HasAlarmProperty =
        DependencyProperty.Register(
            nameof(HasAlarm),
            typeof(bool),
            typeof(WorldClockCardControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsAlarmRingingProperty =
        DependencyProperty.Register(
            nameof(IsAlarmRinging),
            typeof(bool),
            typeof(WorldClockCardControl),
            new PropertyMetadata(false, OnIsAlarmRingingChanged));

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

    public bool HasAlarm
    {
        get => (bool)GetValue(HasAlarmProperty);
        set => SetValue(HasAlarmProperty, value);
    }

    public bool IsAlarmRinging
    {
        get => (bool)GetValue(IsAlarmRingingProperty);
        set => SetValue(IsAlarmRingingProperty, value);
    }

    private static void OnIsAlarmRingingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (WorldClockCardControl)d;
        var ringing = (bool)e.NewValue;
        if (control.CardBorder is null)
        {
            return;
        }

        control.CardBorder.BorderBrush = ringing ? control.NeonRingBrush() : control.DefaultBorderBrush();
    }

    private Brush DefaultBorderBrush() =>
        Application.Current.Resources.TryGetValue("CardBorderBrush", out var resource) &&
        resource is Brush brush
            ? brush
            : new SolidColorBrush(Color.FromArgb(255, 0x3A, 0x3A, 0x3A));

    private Brush NeonRingBrush() =>
        Application.Current.Resources.TryGetValue("NeonAccentBrush", out var resource) &&
        resource is Brush brush
            ? brush
            : new SolidColorBrush(Colors.Cyan);
}