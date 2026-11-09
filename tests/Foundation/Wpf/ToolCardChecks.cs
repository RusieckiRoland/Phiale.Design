using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Phiale.Design.Wpf;

namespace Foundation.Wpf;
public partial class MainWindow
{
    private async Task CheckToolCards()
    {
        Style ToolStyle(string name) => (Style)FindResource("Phiale.Design.Style.ToolCard." + name);
        var surface = new Border { Style = ToolStyle("Surface"), Padding = new Thickness(10, 8, 10, 8) };
        var header = new Border { Style = ToolStyle("Header") };
        var layout = new DockPanel { Style = ToolStyle("HeaderLayout"), LastChildFill = false };
        var grip = new TextBlock { Style = ToolStyle("DragGrip") };
        var title = new TextBlock { Style = ToolStyle("Title"), Text = "Tool card" };
        var toggle = new Button { Style = ToolStyle("ToggleButton"), Content = ">" };
        var close = new Button { Style = ToolStyle("CloseButton") };
        var body = new TextBox { Text = "Application content" };
        layout.Children.Add(grip); layout.Children.Add(toggle); layout.Children.Add(title);
        DockPanel.SetDock(close, Dock.Right); layout.Children.Add(close);
        header.Child = layout;
        var grid = new Grid(); grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); grid.RowDefinitions.Add(new RowDefinition());
        grid.Children.Add(header); Grid.SetRow(body, 1); grid.Children.Add(body); surface.Child = grid;
        var window = new Window { Width = 350, Height = 220, Content = surface, ShowInTaskbar = false };
        var clicks = 0; close.Click += (_, _) => clicks++; toggle.Click += (_, _) => clicks++;
        try {
            window.Show();
            foreach (var mode in new[] { "Light", "Dark", "HighContrast" }) {
                await SetMode(window, mode); window.UpdateLayout();
                Require(surface.CornerRadius == new CornerRadius(12), "ToolCard surface metric.");
                Require(header.CornerRadius == new CornerRadius(8), "ToolCard header metric.");
                Require(layout.MinHeight == 34 && header.ActualHeight >= 34, "ToolCard header height.");
                Require(header.Padding == new Thickness(8, 0, 8, 0), "ToolCard header padding.");
                Require(toggle.Width == 22 && toggle.Height == 22, "ToolCard toggle dimensions.");
                toggle.ApplyTemplate(); close.ApplyTemplate();
                Require(((Border)VisualTreeHelper.GetChild(toggle, 0)).CornerRadius == new CornerRadius(11), "ToolCard circular toggle.");
                RequireBrush(header.Background, "Surface.Header", mode);
                RequireBrush(surface.Background, "Surface.Base", mode);
                RequireBrush(title.Foreground, "Text.Secondary", mode);
                RequireBrush(grip.Foreground, "Text.Muted", mode);
                SetPointerOver(close, true); RequireBrush(close.Background, "Hover.Fill", mode);
                SetPointerOver(close, false); close.IsEnabled = false;
                RequireBrush(close.Foreground, "Disabled.Text", mode); close.IsEnabled = true;
                Require(close.Focus(), "ToolCard action cannot focus.");
                close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Require(body.Style == null && body.Text == "Application content", "ToolCard changed application content.");
                var image = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                image.Render(window); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(image));
                Directory.CreateDirectory(Output);
                using (var stream = File.Create(Path.Combine(Output, "tool-card-" + mode + ".png"))) encoder.Save(stream);
            }
            Require(clicks == 6, "ToolCard consumed application actions.");
            window.Resources["Phiale.Design.Radius.ToolCard.Header"] = new CornerRadius(5);
            await Dispatcher.InvokeAsync(() => window.UpdateLayout(), DispatcherPriority.ApplicationIdle);
            Require(header.CornerRadius == new CornerRadius(5), "ToolCard radius override is not dynamic.");
        } finally { window.Close(); }
    }
}
