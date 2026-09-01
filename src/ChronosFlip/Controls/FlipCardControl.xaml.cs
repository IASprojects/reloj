using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Controls;

public sealed partial class FlipCardControl : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(FlipCardControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(FlipCardControl),
            new PropertyMetadata(string.Empty));

    public FlipCardControl()
    {
        InitializeComponent();
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}