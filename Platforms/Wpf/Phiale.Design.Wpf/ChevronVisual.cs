using System.Windows;

namespace Phiale.Design.Wpf;

public enum ChevronDirection { Left, Right, Up, Down }

/// <summary>Visual direction only. The consumer supplies the panel state and command.</summary>
public static class ChevronVisual
{
    public static readonly DependencyProperty DirectionProperty = DependencyProperty.RegisterAttached(
        "Direction", typeof(ChevronDirection), typeof(ChevronVisual),
        new FrameworkPropertyMetadata(ChevronDirection.Right));

    public static ChevronDirection GetDirection(DependencyObject element) => (ChevronDirection)element.GetValue(DirectionProperty);
    public static void SetDirection(DependencyObject element, ChevronDirection value) => element.SetValue(DirectionProperty, value);
}
