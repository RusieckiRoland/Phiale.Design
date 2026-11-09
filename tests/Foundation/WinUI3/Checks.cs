using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Foundation.WinUI;
public partial class App : Application
{
    public App() => InitializeComponent();
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        window.Activate();
    }
}
public sealed partial class MainWindow : Window
{
    private int checks;
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(960, 1100));
        Root.Loaded += async (_, _) =>
        {
            try { await Check(); Application.Current.Exit(); }
            catch (Exception error) { Report(error.ToString()); Application.Current.Exit(); }
        };
    }
    private void Require(bool condition, string message)
    {
        checks++;
        if (!condition) throw new InvalidOperationException(message);
    }
    internal static string Output => Environment.GetEnvironmentVariable("PHIALE_DESIGN_RESULT") ?? Path.GetTempPath();
    private static void Report(string text)
    {
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output, "winui.txt"), text);
    }
    private async Task Flush()
    {
        Root.UpdateLayout();
        var ready = new TaskCompletionSource<bool>();
        Root.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => ready.SetResult(true));
        await ready.Task;
        await Task.Delay(30);
    }
    private static Windows.UI.Color ColorOf(Brush brush) => ((SolidColorBrush)brush).Color;
    private static T? Descendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T found) return found;
            var nested = Descendant<T>(child);
            if (nested != null) return nested;
        }
        return null;
    }
    private async Task Check()
    {
        await Flush();
        var hc = new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast;
        Require(HostButton.Style == null && HostText.Style == null, "Import changed host styles.");
        for (var i = 0; i < 22; i++)
        {
            var sample = (FrameworkElement)Root.FindName("Sample" + i);
            Require(sample.Style != null, "Missing explicit style " + i);
            if (sample is Control control) { control.ApplyTemplate(); Require(control.Template != null, "Missing template " + i); }
        }
        var contrastStyle = (Style)Application.Current.Resources["Phiale.Design.Style.ComboBox.HighContrastBlack"];
        var contrast = new ComboBox { Style = contrastStyle, RequestedTheme = ElementTheme.Dark };
        contrast.Items.Add("contrast first"); contrast.Items.Add("contrast second"); contrast.SelectedIndex = 0;
        OtherScope.Children.Add(contrast); await Flush();
        Require(ColorOf(contrast.Background) == Colors.Black, "Explicit contrast ComboBox background is not black.");
        Require(ColorOf(contrast.Foreground) == Colors.White, "Explicit contrast ComboBox text is not white.");
        Require(ColorOf(contrast.BorderBrush) == Colors.White, "Explicit contrast ComboBox border is not white.");
        Require(ColorOf(contrast.FocusVisualPrimaryBrush) == Colors.Yellow, "Explicit contrast ComboBox focus is not yellow.");
        contrast.IsDropDownOpen = true; await Flush(); Require(contrast.IsDropDownOpen, "Contrast dropdown did not open.");
        contrast.SelectedIndex = 1; contrast.IsDropDownOpen = false;
        Require(contrast.SelectedIndex == 1, "Contrast selection did not survive popup closure.");
        OtherScope.Children.Remove(contrast);
        Require(hc || ColorOf(OverrideButton.Background) == Colors.Orange, "Local resource override missing.");
        var other = ColorOf(OtherButton.Background);
        foreach (var mode in new[] { ElementTheme.Light, ElementTheme.Dark })
        {
            Scope.RequestedTheme = mode;
            await Flush();
            var expected = hc ? new Windows.UI.ViewManagement.UISettings().UIElementColor(Windows.UI.ViewManagement.UIElementType.Highlight)
                : mode == ElementTheme.Light ? Windows.UI.Color.FromArgb(255, 47, 128, 237) : Windows.UI.Color.FromArgb(255, 93, 159, 255);
            Require(ColorOf(Sample15.Background) == expected, mode + " accent");
            Require(ColorOf(OtherButton.Background) == other, "Other scope changed.");
            Sample19.Text = "preserved edit";
            Require(Sample19.Focus(FocusState.Programmatic), "TextBox cannot focus.");
            Sample20.IsDropDownOpen = true;
            await Flush();
            Require(Sample20.IsDropDownOpen, "Popup did not open.");
            Sample20.SelectedIndex = 1;
            Sample20.IsDropDownOpen = false;
            Require(Sample20.SelectedIndex == 1, "Selection lost.");
            Sample18.IsChecked = true;
            await Flush();
            Require(Descendant<ContentPresenter>(Sample18) != null, "Toggle template missing.");
            Sample18.IsChecked = false;
            Require(VisualStateManager.GoToState(Sample15, "PointerOver", false), "Pointer state missing.");
            await Flush();
            Require(Descendant<ContentPresenter>(Sample15)!.Background != null, "Pointer brush missing.");
            VisualStateManager.GoToState(Sample15, "Normal", false);
            Sample14.IsEnabled = false;
            await Flush();
            Require(Math.Abs(Descendant<ContentPresenter>(Sample14)!.Opacity - 0.6) < 0.0001, "Disabled style missing: " + Descendant<ContentPresenter>(Sample14)!.Opacity);
            Sample14.IsEnabled = true;
            await Flush();
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(Root);
            Require(bitmap.PixelWidth > 0 && bitmap.PixelHeight > 0, "Raster empty.");
            using var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            var pixels = await bitmap.GetPixelsAsync();
            var bytes = new byte[pixels.Length];
            using (var reader = DataReader.FromBuffer(pixels)) reader.ReadBytes(bytes);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes);
            await encoder.FlushAsync();
            stream.Seek(0);
            var png = new byte[stream.Size];
            using (var reader = new DataReader(stream)) { await reader.LoadAsync((uint)png.Length); reader.ReadBytes(png); }
            Directory.CreateDirectory(Output);
            File.WriteAllBytes(Path.Combine(Output, "winui-" + (hc ? "HC-" : "") + mode + ".png"), png);
        }
        if (hc) Require(ColorOf(OverrideButton.Background) == new Windows.UI.ViewManagement.UISettings().UIElementColor(Windows.UI.ViewManagement.UIElementType.Highlight), "Light override concealed HC: actual=" + ColorOf(OverrideButton.Background) + ", expected=" + new Windows.UI.ViewManagement.UISettings().UIElementColor(Windows.UI.ViewManagement.UIElementType.Highlight));
        Sample15.Background = new SolidColorBrush(Colors.Magenta);
        Scope.RequestedTheme = ElementTheme.Light;
        await Flush();
        Require(ColorOf(Sample15.Background) == Colors.Magenta, "Instance override lost.");
        Sample15.ClearValue(Control.BackgroundProperty);
        await Flush();
        Require(ColorOf(Sample15.Background) != Colors.Magenta, "Reset failed.");
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 50; i++) { Scope.RequestedTheme = i % 2 == 0 ? ElementTheme.Light : ElementTheme.Dark; await Flush(); }
        Require(Sample19.Text == "preserved edit", "Theme change lost editor state.");
        Require(HostButton.Style == null && HostText.Style == null, "Host styles leaked.");
        Report("PASS checks=" + checks + " hc=" + hc + " theme_50_ms=" + watch.Elapsed.TotalMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " (includes 30ms settle per transition)");
    }
}

