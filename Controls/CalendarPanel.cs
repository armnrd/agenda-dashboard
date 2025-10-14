using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AgendaDashboard.ViewModels;
using AgendaDashboard.Views;

namespace AgendaDashboard.Controls;

public class CalendarPanel : Panel
{
    private readonly TimeSpan _dayStart = TimeSpan.FromHours(0); // Start of the day
    private readonly TimeSpan _dayEnd = TimeSpan.FromHours(24); // End of the day

    // Constructor
    public CalendarPanel()
    {
        // Update the panel every 30 seconds - to keep the current time line updated
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += (_, _) => InvalidateVisual();
        timer.Start();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // Timeline settings
        int hours = (int)(_dayEnd - _dayStart).TotalHours;
        double totalMinutes = (_dayEnd - _dayStart).TotalMinutes;

        var linePen = new Pen(Brushes.LightGray, 1);
        var textBrush = Brushes.Gray;
        var typeface = new Typeface("Segoe UI");

        // Draw background
        for (int i = 0; i <= hours; i++)
        {
            TimeSpan current = _dayStart.Add(TimeSpan.FromHours(i));
            double y = (current.TotalMinutes - _dayStart.TotalMinutes) / totalMinutes * ActualHeight;

            // Draw hour label
            var text = new FormattedText(
                current.ToString(@"hh\:mm"),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                textBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(text, new Point(CalendarView.LeftMargin, y - text.Height / 2));

            // Draw hour line
            dc.DrawLine(linePen, new Point(CalendarView.DateLabelOffset, y), new Point(ActualWidth - CalendarView.RightMargin, y));
        }

        // Draw line for current time
        TimeSpan now = DateTime.Now.TimeOfDay;
        if (now >= _dayStart && now <= _dayEnd)
        {
            double y = (now.TotalMinutes - _dayStart.TotalMinutes) / totalMinutes * ActualHeight;
            var nowText = new FormattedText(
                now.ToString(@"hh\:mm"),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                Brushes.Red,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawLine(new Pen(Brushes.Red, 1), new Point(CalendarView.DateLabelOffset, y),
                new Point(ActualWidth - CalendarView.RightMargin - CalendarView.DateLabelOffset, y));
            dc.DrawText(nowText, new Point(ActualWidth - CalendarView.RightMargin - nowText.Width, y - nowText.Height / 2));
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;

        var eventPairs = Children
            .OfType<FrameworkElement>()
            .Select(elem => (elem, elem.DataContext as GcalEvent))
            .Where(pair => pair.Item2 != null)
            .OrderBy(pair => pair.Item2.Start)
            .ToList();

        // Assign columns for overlapping events
        var columns = new Dictionary<GcalEvent, int>();
        var columnEnds = new List<DateTime>();
        foreach (var (_, evt) in eventPairs)
        {
            int col = 0;
            while (col < columnEnds.Count && evt.Start < columnEnds[col])
                col++;
            if (col == columnEnds.Count)
                columnEnds.Add(evt.End);
            else
                columnEnds[col] = evt.End;
            columns[evt] = col;
        }

        int maxColumns = columnEnds.Count;
        double totalMinutes = (_dayEnd - _dayStart).TotalMinutes;
        double width = (finalSize.Width - CalendarView.EventCardsRightOffset) / maxColumns;

        foreach (var (elem, evt) in eventPairs)
        {
            double top = ((evt.Start.TimeOfDay - _dayStart).TotalMinutes / totalMinutes) * finalSize.Height;
            double height = ((evt.End - evt.Start).TotalMinutes / totalMinutes) * finalSize.Height;
            double left = columns[evt] * width + CalendarView.DateLabelOffset; // Use the offset for date labels

            elem.Measure(new Size(width, height));
            elem.Arrange(new Rect(left, top, width, height));
        }

        return finalSize;
    }
}