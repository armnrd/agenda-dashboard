using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AgendaDashboard;

public partial class GcalView : UserControl
{
    public GcalView()
    {
        InitializeComponent();
        DataContext = new GcalViewModel();
        Loaded += CalendarView_Loaded; // Subscribe to the Loaded event to load events when the window is ready
    }

    private void CalendarView_Loaded(object sender, RoutedEventArgs e)
    {
        // Scroll to the current time on load
        ScrollViewer.ScrollToVerticalOffset(
            (DateTime.Now.TimeOfDay.TotalMinutes / 1440) * ScrollViewer.ScrollableHeight); // 1440 minutes in a day

        // Periodically scroll to the current time
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) }; // Refresh every minute
        timer.Tick += (s, args) =>
        {
            ScrollViewer.ScrollToVerticalOffset((DateTime.Now.TimeOfDay.TotalMinutes / 1440) *
                                                ScrollViewer.ScrollableHeight); // 1440 minutes in a day
        };
        timer.Start();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        (DataContext as GcalViewModel)?.SafeLoadGcalEvents();
    }
}