using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Foundation.Wpf;
public partial class MainWindow
{
    private async Task CheckCheckBoxes()
    {
        var checkbox = new CheckBox { Content = "Inspector", Style = (Style)FindResource("Phiale.Design.Style.CheckBox") };
        var window = new Window { Width = 260, Height = 120, Content = checkbox, ShowInTaskbar = false };
        try {
            window.Show();
            foreach (var mode in new[] { "Light", "Dark", "HighContrast" }) {
                await SetMode(window, mode); window.UpdateLayout(); checkbox.ApplyTemplate();
                var box = (Border)checkbox.Template.FindName("Box", checkbox);
                var mark = (Path)checkbox.Template.FindName("Mark", checkbox);
                var row = (Border)checkbox.Template.FindName("Row", checkbox);
                Require(box.ActualWidth == 16 && box.ActualHeight == 16, "Checkbox geometry.");
                Require(mark.Visibility == Visibility.Collapsed, "Unchecked mark.");
                RequireBrush(checkbox.Foreground, "Text.Primary", mode);
                checkbox.IsChecked = true;
                Require(mark.Visibility == Visibility.Visible, "Checked mark.");
                RequireBrush(box.Background, "Selection.Active.Fill", mode);
                RequireBrush(mark.Stroke, "Selection.Active.Text", mode);
                SetPointerOver(checkbox, true);
                RequireBrush(checkbox.Background, mode == "HighContrast" ? "Selection.Active.Fill" : "Hover.Fill", mode);
                if (mode == "HighContrast") RequireBrush(checkbox.Foreground, "Selection.Active.Text", mode);
                SetPointerOver(checkbox, false);
                Require(checkbox.Focus(), "Checkbox keyboard focus.");
                RequireBrush(row.BorderBrush, mode == "HighContrast" ? "Selection.Active.Text" : "Focus.Ring", mode);
                checkbox.IsEnabled = false;
                RequireBrush(checkbox.Foreground, "Disabled.Text", mode);
                checkbox.IsEnabled = true; checkbox.IsChecked = false;
                Require((string)checkbox.Content == "Inspector", "Checkbox preserves application content.");
                window.Focus(); System.Windows.Input.Keyboard.ClearFocus();
            }
        } finally { window.Close(); }
    }
}
