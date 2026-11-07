using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Calendar = Avalonia.Controls.Calendar;

namespace Phiale.Design.Avalonia
{
    /// <summary>A native calendar whose labels follow an explicit culture without changing process culture.</summary>
    public class LocalizedCalendar : Calendar
    {
        public static readonly StyledProperty<string> LanguageCodeProperty =
            AvaloniaProperty.Register<LocalizedCalendar, string>(nameof(LanguageCode), CultureInfo.CurrentUICulture.Name);
        private bool _refreshLabels = true;

        protected override Type StyleKeyOverride => typeof(Calendar);
        public string LanguageCode { get => GetValue(LanguageCodeProperty); set => SetValue(LanguageCodeProperty, value); }

        public LocalizedCalendar()
        {
            SetCurrentValue(FirstDayOfWeekProperty, GetCulture().DateTimeFormat.FirstDayOfWeek);
            LayoutUpdated += (_, _) => RefreshLabels();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == LanguageCodeProperty)
                SetCurrentValue(FirstDayOfWeekProperty, GetCulture().DateTimeFormat.FirstDayOfWeek);
            if (change.Property == LanguageCodeProperty || change.Property == DisplayDateProperty ||
                change.Property == DisplayModeProperty || change.Property == FirstDayOfWeekProperty || change.Property == SelectedDateProperty)
            {
                _refreshLabels = true;
                InvalidateMeasure();
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs args)
        {
            base.OnApplyTemplate(args);
            _refreshLabels = true;
        }

        private CultureInfo GetCulture() => LanguageCode == string.Empty ? CultureInfo.InvariantCulture
            : string.IsNullOrWhiteSpace(LanguageCode) ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(LanguageCode);

        private void RefreshLabels()
        {
            if (!_refreshLabels) return;
            var item = this.GetVisualDescendants().OfType<CalendarItem>().FirstOrDefault();
            if (item == null) return;
            var header = item.GetVisualDescendants().OfType<Button>().FirstOrDefault(button => button.Name == "PART_HeaderButton");
            if (header == null) return;
            _refreshLabels = false;
            var culture = GetCulture();
            if (DisplayMode == CalendarMode.Month)
            {
                header.Content = DisplayDate.ToString("Y", culture);
                var month = item.GetVisualDescendants().OfType<Grid>().FirstOrDefault(grid => grid.Name == "PART_MonthView");
                if (month != null)
                    foreach (var title in month.Children.Where(child => Grid.GetRow(child) == 0))
                        title.DataContext = culture.DateTimeFormat.ShortestDayNames[(Grid.GetColumn(title) + (int)FirstDayOfWeek) % 7];
            }
            else if (DisplayMode == CalendarMode.Year)
            {
                header.Content = DisplayDate.Year.ToString(culture);
                foreach (var month in item.GetVisualDescendants().OfType<CalendarButton>())
                    if (month.DataContext is DateTime date) month.Content = date.ToString("MMM", culture);
            }
        }
    }
}
