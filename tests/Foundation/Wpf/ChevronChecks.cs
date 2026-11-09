using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Phiale.Design.Wpf;

namespace Foundation.Wpf;
public partial class MainWindow
{
    private async Task CheckChevrons()
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        var buttons = new Button[4];
        var directions = new[] { ChevronDirection.Left, ChevronDirection.Right, ChevronDirection.Up, ChevronDirection.Down };
        var clicks = 0;
        for (var index = 0; index < buttons.Length; index++) {
            var button = new Button { Style = (Style)FindResource("Phiale.Design.Style.ChevronButton"), ToolTip = directions[index].ToString() };
            ChevronVisual.SetDirection(button, directions[index]);
            System.Windows.Automation.AutomationProperties.SetName(button, "Toggle " + directions[index]);
            button.Click += (_, _) => clicks++;
            buttons[index] = button; panel.Children.Add(button);
        }
        var window = new Window { Width = 320, Height = 120, Content = panel, ShowInTaskbar = false };
        try {
            window.Show();
            foreach (var mode in new[] { "Light", "Dark", "HighContrast" }) {
                await SetMode(window, mode); window.UpdateLayout();
                for (var index = 0; index < buttons.Length; index++) {
                    var button = buttons[index]; button.ApplyTemplate();
                    var chevron = (System.Windows.Shapes.Path)button.Template.FindName("Chevron", button);
                    Require(button.ActualWidth == 28 && button.ActualHeight == 28, "Chevron hit target.");
                    Require(chevron.StrokeThickness == 1.5, "Chevron stroke metric.");
                    Require(chevron.StrokeStartLineCap == PenLineCap.Round && chevron.StrokeLineJoin == PenLineJoin.Round, "Chevron line caps.");
                    var figure = PathGeometry.CreateFromGeometry(chevron.Data).Figures[0];
                    Require(figure.StartPoint == (directions[index] == ChevronDirection.Left ? new Point(8,2)
                        : directions[index] == ChevronDirection.Right ? new Point(4,2)
                        : directions[index] == ChevronDirection.Up ? new Point(2,8) : new Point(2,4)), "Chevron direction.");
                    Require(((SolidColorBrush)button.Background).Color.A == 0, "Chevron idle background.");
                    Require(((SolidColorBrush)button.BorderBrush).Color.A == 0, "Chevron idle border.");
                    RequireBrush(button.Foreground, "Text.Primary", mode);
                    SetPointerOver(button, true);
                    RequireBrush(button.Background, mode == "HighContrast" ? "Selection.Active.Fill" : "Hover.Fill", mode);
                    if (mode == "HighContrast") RequireBrush(button.Foreground, "Selection.Active.Text", mode);
                    SetPointerOver(button, false);
                    Require(button.Focus(), "Chevron keyboard focus.");
                    RequireBrush(button.BorderBrush, "Focus.Ring", mode);
                    button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    button.IsEnabled = false;
                    RequireBrush(button.Foreground, "Disabled.Text", mode);
                    button.IsEnabled = true;
                    window.Focus();
                    System.Windows.Input.Keyboard.ClearFocus();
                }
                foreach (var scale in new[] { 1d, 1.5d, 2d }) {
                    var image = new RenderTargetBitmap((int)(window.ActualWidth * scale), (int)(window.ActualHeight * scale), 96 * scale, 96 * scale, PixelFormats.Pbgra32);
                    image.Render(window);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
                    using var stream = File.Create(System.IO.Path.Combine(Output, "chevrons-" + mode + "-" + (int)(100 * scale) + ".png"));
                    encoder.Save(stream);
                }
            }
            Require(clicks == 12, "Chevron consumed application actions.");
        } finally { window.Close(); }
    }
}
