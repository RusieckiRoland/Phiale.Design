using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using NUnit.Framework;
using Phiale.Design.Avalonia;
using Path = System.IO.Path;
using Calendar = Avalonia.Controls.Calendar;

[assembly: AvaloniaTestApplication(typeof(Design.Avalonia.Tests.TestAppBuilder))]
namespace Design.Avalonia.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
public partial class TestApp : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
public partial class TestWindow : Window, IDisposable
{
    public TestWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Calendar.DisplayDate = new DateTime(2026, 11, 18);
        Calendar.SelectedDate = Calendar.DisplayDate;
        Show();
        Flush();
    }
    public LocalizedCalendar Calendar => this.FindControl<LocalizedCalendar>("CalendarView")!;
    public CheckBox Choice => this.FindControl<CheckBox>("ChoiceView")!;
    public ThemeVariantScope Scope => this.FindControl<ThemeVariantScope>("ScopeRoot")!;
    public static void Flush() => Dispatcher.UIThread.RunJobs();
    public void Dispose() => Close();
}

[TestFixture]
public sealed class FoundationControlTests
{
    private static readonly ThemeVariant[] Modes = { ThemeVariant.Light, ThemeVariant.Dark, DesignThemeVariants.HighContrast };
    private static Color ColorOf(IBrush? brush) => ((ISolidColorBrush)brush!).Color;
    private static Color Role(Control control, string role)
    {
        Assert.That(control.TryFindResource("Phiale.Design.Brush." + role, control.ActualThemeVariant, out var value), Is.True, role);
        return ColorOf((IBrush)value!);
    }
    private static CalendarDayButton SelectedDay(Calendar calendar) => calendar.GetVisualDescendants()
        .OfType<CalendarDayButton>().Single(day => day.Classes.Contains(":selected"));
    private static Button Header(Calendar calendar) => calendar.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_HeaderButton");
    private static void Click(TestWindow window, Control target)
    {
        window.UpdateLayout();
        var point = target.TranslatePoint(new Point(target.Bounds.Width / 2, target.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        TestWindow.Flush();
    }

    [AvaloniaTest]
    public void BodyFontLoadsBundledInterWithPolishGlyphsAndRealWeights()
    {
        Assert.That(Application.Current!.TryFindResource("Phiale.Design.Text.Body.FontFamily", out var resource), Is.True);
        var family = (FontFamily)resource!;
        Assert.That(family.Name, Is.EqualTo("Inter"));
        Assert.That(family.Key, Is.Not.Null, "Font must reference bundled assets.");
        foreach (var weight in new[] { FontWeight.Normal, FontWeight.SemiBold, FontWeight.Bold })
        {
            Assert.That(FontManager.Current.TryGetGlyphTypeface(new Typeface(family, weight: weight), out var face), Is.True);
            Assert.That(face!.FamilyName, Does.StartWith("Inter"), "System fallback must not hide a missing font asset.");
            Assert.That(face.Weight, Is.EqualTo(weight));
            foreach (var character in "Zażółć gęślą jaźń ĄĆĘŁŃÓŚŹŻ")
                Assert.That(face.GetGlyph(character), Is.Not.Zero, character.ToString());
        }
    }

    [AvaloniaTest]
    public void ShadowHasSourceAlphaAndIsInvisibleInHighContrast()
    {
        using var window = new TestWindow();
        foreach (var mode in Modes)
        {
            window.Scope.RequestedThemeVariant = mode;
            TestWindow.Flush();
            Assert.That(window.Calendar.TryFindResource("Phiale.Design.Brush.Shadow", mode, out var value), Is.True);
            var brush = (ISolidColorBrush)value!;
            if (mode == DesignThemeVariants.HighContrast)
                Assert.That(brush.Opacity, Is.Zero);
            else
            {
                Assert.That(brush.Opacity, Is.EqualTo(1));
                Assert.That(brush.Color.A, Is.EqualTo(mode == ThemeVariant.Light ? 0x14 : 0x40));
            }
        }
    }

    [AvaloniaTest]
    public void ThemesChangeEffectiveColorsWithoutChangingSelectionOrOtherScopes()
    {
        using var window = new TestWindow();
        var other = window.FindControl<CheckBox>("OtherChoice")!;
        var otherColor = ColorOf(other.Background);
        foreach (var mode in Modes)
        {
            window.Scope.RequestedThemeVariant = mode;
            window.Choice.IsChecked = true;
            TestWindow.Flush();
            Assert.That(ColorOf(window.Choice.Background), Is.EqualTo(Role(window.Choice, "Selection.Active.Fill")));
            Assert.That(ColorOf(window.Calendar.Background), Is.EqualTo(Role(window.Calendar, "Surface.Base")));
            var day = SelectedDay(window.Calendar);
            Assert.That(ColorOf(day.Background), Is.EqualTo(Role(day, "Selection.Active.Fill")));
            Assert.That(ColorOf(day.Foreground), Is.EqualTo(Role(day, "Selection.Active.Text")));
            Assert.That(day.GetVisualDescendants().OfType<Border>().Any(b => b.Name == "PART_Root"), Is.True, "Private day template is not applied.");
            Assert.That(ColorOf(Header(window.Calendar).Foreground), Is.EqualTo(Role(window.Calendar, "Text.Primary")));
            Assert.That(window.Calendar.SelectedDate, Is.EqualTo(new DateTime(2026, 11, 18)));
            Assert.That(ColorOf(other.Background), Is.EqualTo(otherColor));
            var output = Environment.GetEnvironmentVariable("PHIALE_DESIGN_CAPTURE");
            if (!string.IsNullOrEmpty(output))
            {
                Directory.CreateDirectory(output);
                using var bitmap = new RenderTargetBitmap(new PixelSize(1000, 620));
                bitmap.Render(window);
                bitmap.Save(Path.Combine(output, "foundation-" + mode.Key + ".png"));
            }
        }
    }

    [AvaloniaTest]
    public void SharedButtonsExposeKeyboardFocusInEachTheme()
    {
        using var window = new TestWindow();
        var button = window.FindControl<Button>("OpenPopup")!;
        foreach (var mode in Modes)
        {
            window.Scope.RequestedThemeVariant = mode;
            foreach (var variant in new[] { "Primary", "Secondary", "Tertiary", "Danger" })
            {
                button.Theme = (ControlTheme)window.FindResource("Phiale.Design.Style.Button." + variant)!;
                ((IPseudoClasses)button.Classes).Set(":focus-visible", true);
                TestWindow.Flush();
                var presenter = button.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == "PART_ContentPresenter");
                Assert.That(ColorOf(presenter.BorderBrush), Is.EqualTo(Role(button, "Focus.Ring")), variant);
                ((IPseudoClasses)button.Classes).Set(":focus-visible", false);
            }
        }
    }

    [AvaloniaTest]
    public void CheckBoxWithoutLabelCentersIndicatorAndStillAcceptsALabelLater()
    {
        using var window = new TestWindow();
        var check = window.Choice;
        check.HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Center;
        check.Width = 180;
        check.Content = null;
        TestWindow.Flush();
        var indicator = check.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_Indicator");
        var presenter = check.GetVisualDescendants().OfType<ContentPresenter>().Single();
        Assert.That(presenter.IsVisible, Is.False);
        var center = indicator.TranslatePoint(new Point(indicator.Bounds.Width / 2, 0), check)!.Value;
        Assert.That(center.X, Is.EqualTo(check.Bounds.Width / 2).Within(.5));
        check.Content = "Label";
        TestWindow.Flush();
        Assert.That(presenter.IsVisible, Is.True);
        Assert.That(presenter.Bounds.Width, Is.GreaterThan(0));
        Assert.That(presenter.TranslatePoint(default, check)!.Value.X,
            Is.GreaterThan(indicator.TranslatePoint(new Point(indicator.Bounds.Width, 0), check)!.Value.X));
    }

    [AvaloniaTest]
    public void CheckBoxRetainsNativeInputAndThreeStateBehavior()
    {
        using var window = new TestWindow();
        window.Choice.Focus();
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);
        TestWindow.Flush();
        Assert.That(window.Choice.IsChecked, Is.True);
        var check = window.Choice.GetVisualDescendants().OfType<global::Avalonia.Controls.Shapes.Path>().Single(p => p.Name == "PART_Check");
        Assert.That(check.IsVisible, Is.True);
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);
        TestWindow.Flush();
        Assert.That(window.Choice.IsChecked, Is.Null);
        Assert.That(check.IsVisible, Is.False);
        Assert.That(window.Choice.GetVisualDescendants().OfType<global::Avalonia.Controls.Shapes.Path>().Single(p => p.Name == "PART_Mixed").IsVisible, Is.True);
        window.KeyPressQwerty(PhysicalKey.Space, RawInputModifiers.None);
        window.KeyReleaseQwerty(PhysicalKey.Space, RawInputModifiers.None);
        Assert.That(window.Choice.IsChecked, Is.False);
    }

    [AvaloniaTest]
    public void HoverFocusAndDisabledUseSemanticRolesInEveryTheme()
    {
        using var window = new TestWindow();
        foreach (var mode in Modes)
        {
            window.Scope.RequestedThemeVariant = mode;
            var states = (IPseudoClasses)window.Choice.Classes;
            states.Set(":pointerover", true);
            states.Set(":focus-visible", true);
            TestWindow.Flush();
            Assert.That(ColorOf(window.Choice.Background), Is.EqualTo(Role(window.Choice, "Hover.Fill")));
            Assert.That(ColorOf(window.Choice.BorderBrush), Is.EqualTo(Role(window.Choice, "Focus.Ring")));
            window.Choice.IsEnabled = false;
            TestWindow.Flush();
            Assert.That(ColorOf(window.Choice.Foreground), Is.EqualTo(Role(window.Choice, "Disabled.Text")));
            Assert.That(ColorOf(window.Choice.Background), Is.EqualTo(Role(window.Choice, "Disabled.Fill")));
            window.Choice.IsEnabled = true;
            states.Set(":pointerover", false);
            states.Set(":focus-visible", false);
        }
    }

    [AvaloniaTest]
    public void ButtonThemesUseDisabledAndDangerHoverRolesOnTheActualPresenter()
    {
        using var window = new TestWindow();
        var button = window.FindControl<Button>("OpenPopup")!;
        foreach (var mode in Modes)
        {
            window.Scope.RequestedThemeVariant = mode;
            foreach (var name in new[] { "Primary", "Secondary", "Tertiary", "Danger" })
            {
                button.Theme = (ControlTheme)button.FindResource("Phiale.Design.Style.Button." + name)!;
                button.IsEnabled = false;
                TestWindow.Flush();
                var presenter = button.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == "PART_ContentPresenter");
                Assert.That(ColorOf(presenter.Background), Is.EqualTo(Role(button, "Disabled.Fill")), mode + " " + name);
                Assert.That(ColorOf(presenter.Foreground), Is.EqualTo(Role(button, "Disabled.Text")), mode + " " + name);
                Assert.That(button.Opacity, Is.EqualTo(1), "Do not dim an already disabled semantic color a second time.");
                button.IsEnabled = true;
                if (name == "Danger")
                {
                    ((IPseudoClasses)button.Classes).Set(":pointerover", true);
                    TestWindow.Flush();
                    Assert.That(ColorOf(presenter.Background), Is.EqualTo(Role(button, "Danger.Hover.Fill")));
                    Assert.That(ColorOf(presenter.BorderBrush), Is.EqualTo(Role(button, "Danger.Hover.Border")));
                    ((IPseudoClasses)button.Classes).Set(":pointerover", false);
                }
            }
        }
    }

    [AvaloniaTest]
    public void CalendarUsesItsCultureAndRefreshesAfterRetemplate()
    {
        var original = CultureInfo.CurrentCulture;
        var originalUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            using var window = new TestWindow();
            foreach (var language in new[] { "pl-PL", "en-US", "" })
            {
                var culture = language.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(language);
                window.Calendar.LanguageCode = language;
                TestWindow.Flush();
                Assert.That(Header(window.Calendar).Content, Is.EqualTo(window.Calendar.DisplayDate.ToString("Y", culture)));
                Assert.That(window.Calendar.FirstDayOfWeek, Is.EqualTo(culture.DateTimeFormat.FirstDayOfWeek));
                var template = window.Calendar.Template;
                window.Calendar.Template = null;
                TestWindow.Flush();
                window.Calendar.Template = template;
                TestWindow.Flush();
                Assert.That(Header(window.Calendar).Content, Is.EqualTo(window.Calendar.DisplayDate.ToString("Y", culture)));
                Assert.That(CultureInfo.CurrentCulture.Name, Is.EqualTo("fr-FR"));
                window.Calendar.DisplayMode = CalendarMode.Year;
                TestWindow.Flush();
                Assert.That(Header(window.Calendar).Content, Is.EqualTo("2026"));
                var month = window.Calendar.GetVisualDescendants().OfType<CalendarButton>().First(b => b.DataContext is DateTime date && date.Month == 11);
                Assert.That(month.Content, Is.EqualTo(new DateTime(2026, 11, 1).ToString("MMM", culture)));
                window.Calendar.DisplayMode = CalendarMode.Month;
            }
        }
        finally { CultureInfo.CurrentCulture = original; CultureInfo.CurrentUICulture = originalUi; }
    }

    [AvaloniaTest]
    public void CalendarDayClickStillChangesNativeSelection()
    {
        using var window = new TestWindow();
        var target = window.Calendar.GetVisualDescendants().OfType<CalendarDayButton>()
            .Single(b => b.DataContext is DateTime date && date == new DateTime(2026, 11, 20));
        Click(window, target);
        Assert.That(window.Calendar.SelectedDate, Is.EqualTo(new DateTime(2026, 11, 20)));
    }

    [AvaloniaTest]
    public void CalendarNavigationAndMonthSelectionKeepTheNativeBehavior()
    {
        using var window = new TestWindow();
        Button Part(string name) => window.Calendar.GetVisualDescendants().OfType<Button>().Single(b => b.Name == name);
        Click(window, Part("PART_NextButton"));
        Assert.That(window.Calendar.DisplayDate.Month, Is.EqualTo(12));
        Click(window, Part("PART_PreviousButton"));
        Assert.That(window.Calendar.DisplayDate.Month, Is.EqualTo(11));
        Click(window, Header(window.Calendar));
        Assert.That(window.Calendar.DisplayMode, Is.EqualTo(CalendarMode.Year));
        var march = window.Calendar.GetVisualDescendants().OfType<CalendarButton>()
            .Single(b => b.DataContext is DateTime date && date.Month == 3);
        Click(window, march);
        Assert.That(window.Calendar.DisplayMode, Is.EqualTo(CalendarMode.Month));
        Assert.That(window.Calendar.DisplayDate.Month, Is.EqualTo(3));
        Assert.That(window.Calendar.SelectedDate, Is.EqualTo(new DateTime(2026, 11, 18)));
    }

    [AvaloniaTest]
    public void OpenPopupAndLocalOverrideFollowTheirScope()
    {
        using var window = new TestWindow();
        var popup = window.FindControl<Popup>("Popup")!;
        var calendar = window.FindControl<LocalizedCalendar>("PopupCalendar")!;
        popup.IsOpen = true;
        try
        {
            foreach (var mode in Modes)
            {
                window.Scope.RequestedThemeVariant = mode;
                TestWindow.Flush();
                Assert.That(popup.IsOpen, Is.True);
                Assert.That(calendar.ActualThemeVariant, Is.EqualTo(mode));
                Assert.That(ColorOf(calendar.Background), Is.EqualTo(Role(calendar, "Surface.Base")));
            }
            window.Scope.Resources["Phiale.Design.Brush.Surface.Base"] = Brushes.Magenta;
            TestWindow.Flush();
            Assert.That(ColorOf(calendar.Background), Is.EqualTo(Colors.Magenta));
            Assert.That(ColorOf(window.FindControl<LocalizedCalendar>("OtherCalendar")!.Background), Is.Not.EqualTo(Colors.Magenta));
            window.Scope.Resources.Remove("Phiale.Design.Brush.Surface.Base");
            TestWindow.Flush();
            Assert.That(ColorOf(calendar.Background), Is.EqualTo(Role(calendar, "Surface.Base")));
        }
        finally { popup.IsOpen = false; }
    }

    [AvaloniaTest]
    public void DesignDoesNotInstallResourcesOrStyleUnrelatedHostControls()
    {
        var styles = Application.Current!.Styles.Count;
        var dictionaries = Application.Current.Resources.MergedDictionaries.Count;
        using var window = new TestWindow();
        Assert.That(Application.Current.Styles.Count, Is.EqualTo(styles));
        Assert.That(Application.Current.Resources.MergedDictionaries.Count, Is.EqualTo(dictionaries));
        Assert.That(window.FindControl<CheckBox>("HostChoice")!.Theme, Is.Null);
        var host = window.FindControl<Calendar>("HostCalendar")!;
        Assert.That(host.Theme, Is.Null);
        Assert.That(host.GetVisualDescendants().OfType<Border>().Any(b => b.Name == "PART_Root"), Is.False);
        Assert.That(typeof(LocalizedCalendar).Assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("PhialeTech.") || a.Name.StartsWith("PhialeGrid")), Is.False);
    }
}
