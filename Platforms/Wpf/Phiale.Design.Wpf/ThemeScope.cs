using System.Windows;

namespace Phiale.Design.Wpf;

// Inherited selection only; values and visual states are declared in XAML.
public static class ThemeScope
{
    public static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached(
        "Mode", typeof(string), typeof(ThemeScope),
        new FrameworkPropertyMetadata("Light", FrameworkPropertyMetadataOptions.Inherits));

    public static string GetMode(DependencyObject target) => (string)target.GetValue(ModeProperty);
    public static void SetMode(DependencyObject target, string mode) => target.SetValue(ModeProperty, mode);
}
