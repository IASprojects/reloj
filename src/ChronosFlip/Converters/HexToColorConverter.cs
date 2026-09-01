using System.Globalization;
using ChronosFlip.Core.Settings;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ChronosFlip.Converters;

public sealed class HexToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var hex = value as string;
        if (!TryParse(hex, out var color))
        {
            color = ColorHelper.FromArgb(0xFF, 0x00, 0xE5, 0xFF);
        }
        return color;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Color color)
        {
            return ToHex(color);
        }
        if (value is SolidColorBrush brush)
        {
            return ToHex(brush.Color);
        }
        return SettingsDefaults.NeonHexColor;
    }

    public static bool TryParse(string? hex, out Color color)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            color = Colors.Transparent;
            return false;
        }

        var s = hex.Trim().TrimStart('#');
        if (s.Length == 6)
        {
            s = "FF" + s;
        }
        else if (s.Length == 3)
        {
            s = "FF" + new string(new[] { s[0], s[0], s[1], s[1], s[2], s[2] });
        }

        if (s.Length != 8)
        {
            color = Colors.Transparent;
            return false;
        }

        if (!byte.TryParse(s.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a) ||
            !byte.TryParse(s.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
            !byte.TryParse(s.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
            !byte.TryParse(s.AsSpan(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
        {
            color = Colors.Transparent;
            return false;
        }

        color = ColorHelper.FromArgb(a, r, g, b);
        return true;
    }

    public static string ToHex(Color color)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "#{0:X2}{1:X2}{2:X2}{3:X2}",
            color.A, color.R, color.G, color.B);
    }
}
