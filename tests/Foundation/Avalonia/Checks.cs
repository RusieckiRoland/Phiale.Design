using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Phiale.Design.Avalonia;

namespace Foundation.Avalonia;
internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => AppBuilder.Configure<App>().UsePlatformDetect().StartWithClassicDesktopLifetime(args);
}
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;
            window.Opened += async (_, _) =>
            {
                try { await window.Check(); desktop.Shutdown(0); }
                catch (Exception error) { MainWindow.Report(error.ToString()); desktop.Shutdown(1); }
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
public partial class MainWindow : Window
{
    private int checks;
    public MainWindow() => AvaloniaXamlLoader.Load(this);
    internal static string Output => Environment.GetEnvironmentVariable("PHIALE_DESIGN_RESULT") ?? Path.GetTempPath();
    internal static void Report(string text)
    {
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "avalonia.txt"), text);
    }
    private void Require(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }
    private static async Task Flush() => await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
    private IBrush Role(Control scope, string key)
    {
        Require(scope.TryFindResource("Phiale.Design." + key, scope.ActualThemeVariant, out var value) && value is IBrush, "Missing " + key);
        return (IBrush)value!;
    }
    private static Color ColorOf(IBrush? brush) => ((ISolidColorBrush)brush!).Color;
    public async Task Check()
    {
        await Flush();
        var scope = this.FindControl<ThemeVariantScope>("Scope")!;
        var other = this.FindControl<Button>("OtherButton")!;
        var over = this.FindControl<Button>("OverrideButton")!;
        var overScope = this.FindControl<ThemeVariantScope>("OverrideScope")!;
        var host = this.FindControl<Button>("HostButton")!;
        var hostText = this.FindControl<TextBox>("HostText")!;
        var primary = this.FindControl<Button>("Sample15")!;
        var secondary = this.FindControl<Button>("Sample14")!;
        var text = this.FindControl<TextBlock>("Sample0")!;
        var editor = this.FindControl<TextBox>("Sample19")!;
        var combo = this.FindControl<ComboBox>("Sample20")!;
        var toggle = this.FindControl<ToggleButton>("Sample18")!;
        var originalHostTheme = host.Theme;
        var originalHostTextTheme = hostText.Theme;
        for (var i = 0; i < 22; i++)
        {
            var sample = this.FindControl<Control>("Sample" + i)!;
            Require(sample.Theme != null, "Missing explicit theme " + i);
            if (sample is TemplatedControl control) Require(control.Template != null, "Missing native template " + i);
        }
        Require(ColorOf(over.Background) == Colors.Orange, "Local resource override missing.");
        var otherColor = ColorOf(other.Background);
        foreach (var mode in new[] { ThemeVariant.Light, ThemeVariant.Dark, DesignThemeVariants.HighContrast })
        {
            scope.RequestedThemeVariant = mode;
            await Flush();
            Require(ColorOf(primary.Background) == ColorOf(Role(primary, "Brush.Accent")), mode + " accent");
            Require(ColorOf(text.Foreground) == ColorOf(Role(text, "Brush.Text.Primary")), mode + " text");
            Require(ColorOf(other.Background) == otherColor, "Other scope changed.");
            ((IPseudoClasses)primary.Classes).Set(":pointerover", true);
            await Flush();
            var primaryPresenter = primary.GetVisualDescendants().OfType<ContentPresenter>().First();
            Require(ColorOf(primaryPresenter.Background) == ColorOf(Role(primary, "Brush.Accent.Strong")), "Hover did not use the Design accent.");
            Require(ColorOf(primaryPresenter.Foreground) == ColorOf(Role(primary, "Brush.Text.Inverse")), "Hover text contrast pair changed.");
            ((IPseudoClasses)primary.Classes).Set(":pointerover", false);
            editor.Text = "preserved edit";
            editor.Focus();
            await Flush();
            Require(editor.IsFocused, "TextBox cannot focus.");
            Require(ColorOf(editor.Foreground) == ColorOf(Role(editor, "Brush.Text.Primary")), "Focused editor foreground changed to the native palette.");
            Require(ColorOf(editor.GetVisualDescendants().OfType<TextPresenter>().Single().Foreground) == ColorOf(editor.Foreground), "Text presenter ignored the effective foreground.");
            combo.IsDropDownOpen = true;
            await Flush();
            Require(combo.IsDropDownOpen && combo.ContainerFromIndex(0) != null, "Popup not realized.");
            combo.SelectedIndex = 1;
            combo.IsDropDownOpen = false;
            Require(combo.SelectedIndex == 1, "Selection lost.");
            toggle.IsChecked = true;
            await Flush();
            var togglePresenter = toggle.GetVisualDescendants().OfType<ContentPresenter>().First();
            Require(ColorOf(togglePresenter.Background) == ColorOf(Role(toggle, "Brush.Selection.Active.Fill")), "Checked state ignored Design.");
            Require(ColorOf(togglePresenter.Foreground) == ColorOf(Role(toggle, "Brush.Selection.Active.Text")), "Checked text contrast pair changed.");
            toggle.IsChecked = false;
            secondary.IsEnabled = false;
            await Flush();
            Require(ColorOf(secondary.Foreground) == ColorOf(Role(secondary, "Brush.Disabled.Text")), "Disabled text role missing.");
            Require(ColorOf(secondary.Background) == ColorOf(Role(secondary, "Brush.Disabled.Fill")), "Disabled fill role missing.");
            secondary.IsEnabled = true;
            await Flush();
            using var bitmap = new RenderTargetBitmap(new PixelSize(900, 1000));
            bitmap.Render(this);
            Directory.CreateDirectory(Output);
            bitmap.Save(Path.Combine(Output, "avalonia-" + mode.Key + ".png"));
        }
        overScope.RequestedThemeVariant = DesignThemeVariants.HighContrast;
        await Flush();
        Require(ColorOf(over.Background) == Colors.Yellow, "Light override concealed HC.");
        overScope.RequestedThemeVariant = ThemeVariant.Light;
        await Flush();
        Require(ColorOf(over.Background) == Colors.Orange, "Override did not return.");
        primary.Background = Brushes.Magenta;
        scope.RequestedThemeVariant = ThemeVariant.Dark;
        await Flush();
        Require(ReferenceEquals(primary.Background, Brushes.Magenta), "Instance override lost.");
        primary.ClearValue(TemplatedControl.BackgroundProperty);
        await Flush();
        Require(ColorOf(primary.Background) == ColorOf(Role(primary, "Brush.Accent")), "Reset failed.");
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 50; i++) { scope.RequestedThemeVariant = i % 2 == 0 ? ThemeVariant.Light : ThemeVariant.Dark; await Flush(); }
        Require(editor.Text == "preserved edit", "Theme change lost editor state.");
        Require(ReferenceEquals(host.Theme, originalHostTheme) && ReferenceEquals(hostText.Theme, originalHostTextTheme), "Host themes leaked.");
        Report("PASS checks=" + checks + " theme_50_ms=" + watch.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
    }
}

