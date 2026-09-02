using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ChronosFlip.Converters;

/// <summary>Converts a bool to <see cref="Visibility"/> for view affordances.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Visible;
    }
}