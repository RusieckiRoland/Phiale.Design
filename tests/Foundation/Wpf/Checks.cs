using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Phiale.Design.Wpf;

namespace Foundation.Wpf;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var window = new MainWindow();
        window.Show();
        window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(async () =>
        {
            try { await window.Check(); Shutdown(0); }
            catch (Exception error) { Foundation.Wpf.MainWindow.Report(error.ToString()); Shutdown(1); }
        }));
    }
}
public partial class MainWindow : Window
{
    private int checks;
    private readonly Stopwatch timer = Stopwatch.StartNew();
    public MainWindow() => InitializeComponent();
    private void Require(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }
    internal static string Output => Environment.GetEnvironmentVariable("PHIALE_DESIGN_RESULT") ?? Path.GetTempPath();
    internal static void Report(string text)
    {
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "wpf-" + Environment.Version.Major + ".txt"), text);
    }
    private async Task SetMode(FrameworkElement scope, string mode)
    {
        ThemeScope.SetMode(scope, mode);
        await Dispatcher.InvokeAsync(() => UpdateLayout(), DispatcherPriority.ApplicationIdle);
    }
    // State injection verifies native trigger precedence without moving the user's pointer.
    private static void SetPointerOver(UIElement element, bool value)
    {
        var key = (DependencyPropertyKey)typeof(UIElement).GetField("IsMouseOverPropertyKey", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        element.SetValue(key, value);
    }
    private void RequireBrush(Brush actual, string role, string mode)
    {
        Require(((SolidColorBrush)actual).Color == ((SolidColorBrush)FindResource("Phiale.Design.Brush." + role + "." + mode)).Color, mode + " " + role);
    }
    private void CheckInteractionStates(string mode)
    {
        var buttons = new[] { Sample14, Sample15, Sample16, Sample17 };
        var normal = new[] { mode == "Light" ? "Surface.Base" : "Surface.Secondary", "Accent", "Danger.Fill", "Surface.Base" };
        var hover = new[] { "Hover.Fill", "Accent.Strong", "Danger.Hover.Fill", "Hover.Fill" };
        var hoverBorder = new[] { "Border.Strong", "Accent.Strong", "Danger.Hover.Border", "Border.Strong" };
        for (var index = 0; index < buttons.Length; index++)
        {
            var button = buttons[index];
            SetPointerOver(button, false);
            RequireBrush(button.Background, normal[index], mode);
            SetPointerOver(button, true);
            RequireBrush(button.Background, hover[index], mode);
            RequireBrush(button.BorderBrush, hoverBorder[index], mode);
            button.IsEnabled = false;
            Require(button.Opacity == 1, "Disabled colors must not be faded a second time.");
            RequireBrush(button.Background, "Disabled.Fill", mode);
            RequireBrush(button.Foreground, "Disabled.Text", mode);
            RequireBrush(button.BorderBrush, "Disabled.Border", mode);
            SetPointerOver(button, false);
            button.IsEnabled = true;
            RequireBrush(button.Background, normal[index], mode);
        }
        Sample21.ApplyTemplate();
        var track = (Track)Sample21.Template.FindName("PART_Track", Sample21);
        track.Thumb.ApplyTemplate();
        var chrome = (Border)track.Thumb.Template.FindName("ThumbChrome", track.Thumb);
        RequireBrush(Sample21.Background, "Scroll.Track", mode);
        RequireBrush(chrome.Background, "Scroll.Thumb", mode);
        SetPointerOver(track.Thumb, true);
        RequireBrush(chrome.Background, "Scroll.Thumb.Hover", mode);
        SetPointerOver(track.Thumb, false);
        RequireBrush(chrome.Background, "Scroll.Thumb", mode);
    }
    public async Task Check()
    {
        await CheckToolCards();
        await CheckSplitters();
        await CheckChevrons();
        await CheckCheckBoxes();
        foreach (var mode in new[] { "Light", "Dark" }) {
            var workspace = (SolidColorBrush)FindResource("Phiale.Design.Brush.Surface.Workspace." + mode);
            var empty = (SolidColorBrush)FindResource("Phiale.Design.Brush.Surface.Base." + mode);
            Require(workspace.Color != empty.Color, "Workspace and empty host must be distinct in " + mode);
        }
        Require(((SolidColorBrush)FindResource("Phiale.Design.Brush.Surface.Workspace.HighContrast")).Color == SystemColors.WindowColor,
            "Workspace High Contrast must follow the current Windows background.");
        Require((CornerRadius)FindResource("Phiale.Design.Radius.Medium") == new CornerRadius(5), "WPF medium radius must be 5 DIP.");
        Require((Thickness)FindResource("Phiale.Design.Thickness.Border.Default") == new Thickness(1), "Default border must be 1 DIP.");
        Require(HostButton.Style == null && HostText.Style == null, "Import changed host styles.");
        for (var i = 0; i < 22; i++)
        {
            var sample = (FrameworkElement)FindName("Sample" + i);
            Require(sample.Style != null, "Missing explicit style " + i);
            if (sample is Control control) Require(control.ApplyTemplate() || control.Template != null, "Missing template " + i);
        }
        Require(((SolidColorBrush)OverrideButton.Background).Color == Colors.Orange, "Local resource override missing.");
        var other = ((SolidColorBrush)OtherButton.Background).Color;
        foreach (var mode in new[] { "Light", "Dark", "HighContrast" })
        {
            await SetMode(Scope, mode);
            Require(((SolidColorBrush)Sample15.Background).Color == ((SolidColorBrush)FindResource("Phiale.Design.Brush.Accent." + mode)).Color, mode + " accent");
            Require(((SolidColorBrush)Sample0.Foreground).Color == ((SolidColorBrush)FindResource("Phiale.Design.Brush.Text.Primary." + mode)).Color, mode + " text");
            Require(((SolidColorBrush)OtherButton.Background).Color == other, "Other scope changed.");
            Sample19.Text = "preserved edit";
            Sample19.Focus();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            Require(Sample19.IsKeyboardFocusWithin, "TextBox cannot focus.");
            Require(((SolidColorBrush)Sample19.BorderBrush).Color == ((SolidColorBrush)FindResource("Phiale.Design.Brush.Focus.Ring." + mode)).Color, "Focus role missing.");
            Sample20.IsDropDownOpen = true;
            await Dispatcher.InvokeAsync(() => UpdateLayout(), DispatcherPriority.ApplicationIdle);
            Require(Sample20.IsDropDownOpen && Sample20.ItemContainerGenerator.ContainerFromIndex(0) is ComboBoxItem, "Popup not realized.");
            Sample20.SelectedIndex = 1;
            Sample20.IsDropDownOpen = false;
            Require(Sample20.SelectedIndex == 1, "Selection lost.");
            Sample18.IsChecked = true;
            Require(((SolidColorBrush)Sample18.Background).Color == ((SolidColorBrush)FindResource("Phiale.Design.Brush.Selection.Active.Fill." + mode)).Color, "Toggle state missing.");
            Sample18.IsChecked = false;
            CheckInteractionStates(mode);
            var bitmap = new RenderTargetBitmap(900, 1000, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(Root);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            Directory.CreateDirectory(Output);
            using (var stream = File.Create(Path.Combine(Output, "wpf-" + mode + ".png"))) encoder.Save(stream);
        }
        await SetMode(OverrideScope, "HighContrast");
        Require(((SolidColorBrush)OverrideButton.Background).Color == SystemColors.HighlightColor, "Light override concealed HC.");
        await SetMode(OverrideScope, "Light");
        Require(((SolidColorBrush)OverrideButton.Background).Color == Colors.Orange, "Override did not return.");
        Sample15.Background = Brushes.Magenta;
        await SetMode(Scope, "Light");
        await SetMode(Scope, "Dark");
        Require(ReferenceEquals(Sample15.Background, Brushes.Magenta), "Instance override lost.");
        Sample15.ClearValue(Control.BackgroundProperty);
        Require(((SolidColorBrush)Sample15.Background).Color == ((SolidColorBrush)FindResource("Phiale.Design.Brush.Accent.Dark")).Color, "Reset failed.");
        foreach (var orientation in new[] { Orientation.Horizontal, Orientation.Vertical })
        {
            Sample21.Orientation = orientation;
            Sample21.Value = 25;
            Sample21.LargeChange = 10;
            Sample21.ApplyTemplate();
            var track = (Track)Sample21.Template.FindName("PART_Track", Sample21);
            var increase = (RoutedCommand)track.IncreaseRepeatButton.Command;
            Require(increase.CanExecute(null, track.IncreaseRepeatButton), orientation + " page increase is disabled.");
            increase.Execute(null, track.IncreaseRepeatButton);
            Require(Sample21.Value == 35, orientation + " page increase did not scroll.");
            var decrease = (RoutedCommand)track.DecreaseRepeatButton.Command;
            Require(decrease.CanExecute(null, track.DecreaseRepeatButton), orientation + " page decrease is disabled.");
            decrease.Execute(null, track.DecreaseRepeatButton);
            Require(Sample21.Value == 25, orientation + " page decrease did not scroll.");
        }
        Sample21.Orientation = Orientation.Horizontal;
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 50; i++) await SetMode(Scope, i % 2 == 0 ? "Light" : "Dark");
        Require(Sample19.Text == "preserved edit", "Theme change lost editor state.");
        Require(HostButton.Style == null && HostText.Style == null, "Host styles leaked.");
        Report("PASS checks=" + checks + " theme_50_ms=" + watch.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " total_ms=" + timer.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
    }
}

