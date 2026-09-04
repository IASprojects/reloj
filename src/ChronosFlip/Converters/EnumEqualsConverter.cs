using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ChronosFlip.Converters;

/// <summary>
/// Converts an enum value (e.g. <see cref="ChronosFlip.Core.Navigation.MainNavigationPage"/>)
/// against a parameter string: Visibility for view stacking, Brush for the nav-rail
/// selected highlight (feature 08). Existing tokens only.
/// </summary>
public sealed class EnumEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var equal = value is Enum page &&
                    parameter is string expected &&
                    string.Equals(page.ToString(), expected, StringComparison.Ordinal);

        if (targetType == typeof(Visibility))
        {
            return equal ? Visibility.Visible : Visibility.Collapsed;
        }

        if (targetType == typeof(Brush))
        {
            if (Application.Current?.Resources is { } resources &&
                resources.TryGetValue(equal ? "NeonAccentBrush" : "TextOnCardBrush", out var brush) &&
                brush is Brush b)
            {
                return b;
            }
        }

        return equal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}