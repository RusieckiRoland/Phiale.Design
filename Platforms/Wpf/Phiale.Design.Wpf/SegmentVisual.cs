using System.Windows;
namespace Phiale.Design.Wpf;

public enum SegmentPosition { Single, First, Middle, Last }

/// <summary>Presentation state supplied by consumers. All appearance is declared in XAML.</summary>
public static class SegmentVisual
{
    public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached(
        "Position", typeof(SegmentPosition), typeof(SegmentVisual), new FrameworkPropertyMetadata(SegmentPosition.Single));
    public static SegmentPosition GetPosition(DependencyObject value) => (SegmentPosition)value.GetValue(PositionProperty);
    public static void SetPosition(DependencyObject value, SegmentPosition position) => value.SetValue(PositionProperty, position);
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
        "IsSelected", typeof(bool), typeof(SegmentVisual), new FrameworkPropertyMetadata(false));
    public static bool GetIsSelected(DependencyObject value) => (bool)value.GetValue(IsSelectedProperty);
    public static void SetIsSelected(DependencyObject value, bool selected) => value.SetValue(IsSelectedProperty, selected);
}
