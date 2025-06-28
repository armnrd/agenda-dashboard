using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AgendaDashboard;

public class GcalPanel : Panel
{
    private readonly TimeSpan _dayStart = TimeSpan.FromHours(0); // Start of the day
    private readonly TimeSpan _dayEnd = TimeSpan.FromHours(24); // End of the day

    // Constructor
    public GcalPanel()
    {
        // Update the panel every 30 seconds - to keep the current time line updated
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        timer.Tick += (s, e) => InvalidateVisual();
        timer.Start();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // Timeline settings
        int hours = (int)(_dayEnd - _dayStart).TotalHours;
        double totalMinutes = (_dayEnd - _dayStart).TotalMinutes;
        double width = ActualWidth;
        double height = ActualHeight;

        var linePen = new Pen(Brushes.LightGray, 1);
        var textBrush = Brushes.Gray;
        var typeface = new Typeface("Segoe UI");
        double labelMargin = 4;

        // Draw background
        for (int i = 0; i <= hours; i++)
        {
            TimeSpan current = _dayStart.Add(TimeSpan.FromHours(i));
            double y = (current.TotalMinutes - _dayStart.TotalMinutes) / totalMinutes * height;

            // Draw hour line
            dc.DrawLine(linePen, new Point(40, y),
                new Point(width - 10, y)); // 40px for left margin and 10px for right margin

            // Draw hour label
            var text = new FormattedText(
                current.ToString(@"hh\:mm"),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                textBrush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(text, new Point(labelMargin, y - text.Height / 2));
        }

        // Draw line for current time
        TimeSpan now = DateTime.Now.TimeOfDay;
        if (now >= _dayStart && now <= _dayEnd)
        {
            double y = (now.TotalMinutes - _dayStart.TotalMinutes) / totalMinutes * height;
            dc.DrawLine(new Pen(Brushes.Red, 1), new Point(40, y), new Point(width - 10, y));
            var nowText = new FormattedText(
                now.ToString(@"hh\:mm"),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                Brushes.Red,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(nowText, new Point(labelMargin, y - nowText.Height / 2));
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0) return finalSize;

        var events = Children
            .OfType<FrameworkElement>()
            .Select(e => e.DataContext as GcalEvent)
            .Where(e => e != null)
            .OrderBy(e => e.Start)
            .ToList();

        // Assign columns for overlapping events
        var columns = new Dictionary<GcalEvent, int>();
        var columnEnds = new List<DateTime>();
        foreach (var ev in events)
        {
            int col = 0;
            while (col < columnEnds.Count && ev.Start < columnEnds[col])
                col++;
            if (col == columnEnds.Count)
                columnEnds.Add(ev.End);
            else
                columnEnds[col] = ev.End;
            columns[ev] = col;
        }

        int maxColumns = columnEnds.Count;

        double totalMinutes = (_dayEnd - _dayStart).TotalMinutes;


        double width = (finalSize.Width - 50) / maxColumns; // Subtract margin for the timeline
        foreach (var ev in events)
        {
            var child = Children
                .OfType<FrameworkElement>()
                .First(e => e.DataContext == ev);

            double top = ((ev.Start.TimeOfDay - _dayStart).TotalMinutes / totalMinutes) * finalSize.Height;
            double height = ((ev.End - ev.Start).TotalMinutes / totalMinutes) * finalSize.Height;
            double left = columns[ev] * width + 40; // Add a small margin

            child.Measure(new Size(width, height));
            child.Arrange(new Rect(left, top, width, height));
        }

        return finalSize;
    }
}