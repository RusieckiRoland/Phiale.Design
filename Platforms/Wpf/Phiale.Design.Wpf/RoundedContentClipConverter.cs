using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Phiale.Design.Wpf;

/// <summary>Clips popup content inside a uniform rounded outline supplied by XAML.</summary>
public sealed class RoundedContentClipConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 4 || values[0] is not double width || values[1] is not double height
            || values[2] is not CornerRadius radius || values[3] is not Thickness border
            || double.IsNaN(width) || double.IsNaN(height) || double.IsInfinity(width) || double.IsInfinity(height))
            return Geometry.Empty;
        var inset = Math.Max(Math.Max(border.Left, border.Right), Math.Max(border.Top, border.Bottom));
        var innerRadius = Math.Max(0, radius.TopLeft - inset);
        return new RectangleGeometry(new Rect(0, 0, Math.Max(0, width), Math.Max(0, height)), innerRadius, innerRadius);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}