using System.Windows;
using System.Windows.Controls;

namespace Phiale.Design.Wpf;

/// <summary>Orientation of the shared splitter visual. Resize behavior belongs to the consumer.</summary>
public static class SplitterVisual
{
    public static readonly DependencyProperty IsIntegratedProperty = DependencyProperty.RegisterAttached(
        "IsIntegrated", typeof(bool), typeof(SplitterVisual), new FrameworkPropertyMetadata(false));
    public static bool GetIsIntegrated(DependencyObject element) => (bool)element.GetValue(IsIntegratedProperty);
    public static void SetIsIntegrated(DependencyObject element, bool value) => element.SetValue(IsIntegratedProperty, value);
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached(
        "Orientation", typeof(Orientation), typeof(SplitterVisual), new FrameworkPropertyMetadata(Orientation.Vertical));
    public static Orientation GetOrientation(DependencyObject element) => (Orientation)element.GetValue(OrientationProperty);
    public static void SetOrientation(DependencyObject element, Orientation value) => element.SetValue(OrientationProperty, value);
}
