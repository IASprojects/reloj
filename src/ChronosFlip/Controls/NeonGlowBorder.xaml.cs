using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ChronosFlip.Controls;

public sealed partial class NeonGlowBorder : UserControl
{
    public NeonGlowBorder()
    {
        InitializeComponent();
        UpdateVisibility();
    }

    public bool IsNeonEnabled
    {
        get => (bool)GetValue(IsNeonEnabledProperty);
        set => SetValue(IsNeonEnabledProperty, value);
    }

    public object? InnerContent
    {
        get => GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public static readonly DependencyProperty IsNeonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsNeonEnabled),
            typeof(bool),
            typeof(NeonGlowBorder),
            new PropertyMetadata(false, OnIsNeonEnabledChanged));

    public static readonly DependencyProperty InnerContentProperty =
        DependencyProperty.Register(
            nameof(InnerContent),
            typeof(object),
            typeof(NeonGlowBorder),
            new PropertyMetadata(null));

    public static readonly DependencyProperty AccentColorProperty =
        DependencyProperty.Register(
            nameof(AccentColor),
            typeof(Color),
            typeof(NeonGlowBorder),
            new PropertyMetadata(Color.FromArgb(0xFF, 0x00, 0xE5, 0xFF), OnAccentColorChanged));

    private static void OnIsNeonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NeonGlowBorder border)
        {
            border.UpdateVisibility();
        }
    }

    private static void OnAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NeonGlowBorder border)
        {
            border.ApplyAccent((Color)e.NewValue);
        }
    }

    private void UpdateVisibility()
    {
        var visible = IsNeonEnabled ? Visibility.Visible : Visibility.Collapsed;
        GlowOuter.Visibility = visible;
        GlowMid.Visibility = visible;
        GlowNear.Visibility = visible;
        GlowCore.Visibility = visible;
        StrokeBorder.Visibility = visible;
    }

    private void ApplyAccent(Color color)
    {
        var brush = new SolidColorBrush(color);
        GlowOuter.BorderBrush = brush;
        GlowMid.BorderBrush = brush;
        GlowNear.BorderBrush = brush;
        GlowCore.BorderBrush = brush;
        StrokeBorder.BorderBrush = brush;
    }
}
