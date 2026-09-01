using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Controls;

public sealed partial class NeonGlowBorder : UserControl
{
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

    private static void OnIsNeonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NeonGlowBorder border)
        {
            border.UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        GlowBorder.Visibility = IsNeonEnabled ? Visibility.Visible : Visibility.Collapsed;
        StrokeBorder.Visibility = IsNeonEnabled ? Visibility.Visible : Visibility.Collapsed;
    }
}
