using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Phiale.Design.Wpf;

namespace Foundation.Wpf;
public partial class MainWindow
{
    private async Task CheckSplitters()
    {
        var splitter = new Thumb { Style = (Style)FindResource("Phiale.Design.Style.ToolCard.Splitter") };
        var window = new Window { Width = 250, Height = 250, Content = splitter, ShowInTaskbar = false };
        try {
            window.Show();
            foreach (var mode in new[] { "Light", "Dark", "HighContrast" }) {
                await SetMode(window, mode);
                window.UpdateLayout();
                splitter.ApplyTemplate();
                var grip = (Border)splitter.Template.FindName("Grip", splitter);
                Require(splitter.Width == 12 && grip.Width == 10 && grip.Height == 28, "Shared splitter dimensions.");
                RequireBrush(splitter.Background, "Surface.Base", mode);
                RequireBrush(splitter.Foreground, "Text.Muted", mode);
                Require(splitter.Focus(), "Shared splitter cannot receive keyboard focus.");
                SplitterVisual.SetOrientation(splitter, Orientation.Horizontal);
                window.UpdateLayout();
                Require(splitter.Height == 12 && double.IsNaN(splitter.Width), "Shared horizontal splitter dimensions.");
                Require(grip.LayoutTransform.Value.M12 == 1, "Horizontal splitter must rotate the shared grip.");
                SplitterVisual.SetIsIntegrated(splitter, true);
                window.UpdateLayout();
                Require(grip.Width == 6, "Integrated splitter grip fits the eight DIP hit region.");
                var track = (Border)splitter.Template.FindName("Track", splitter);
                Require(track.Margin == new Thickness(0), "Integrated horizontal boundary has no end gaps.");
                SplitterVisual.SetOrientation(splitter, Orientation.Vertical);
                Require(track.Margin == new Thickness(0), "Integrated vertical boundary has no end gaps.");
                SplitterVisual.SetIsIntegrated(splitter, false);
            }
            Require((double)FindResource("Phiale.Design.Size.ToolCard.MinWidth") == 200, "Shared card minimum width.");
        } finally { window.Close(); }
    }
}
