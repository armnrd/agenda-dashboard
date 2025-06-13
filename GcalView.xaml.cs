using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AgendaDashboard;

public partial class GcalView : UserControl
{
    public GcalView()
    {
        InitializeComponent();
        Loaded += CalendarView_Loaded; // Subscribe to the Loaded event to load events when the window is ready
        DataContext = new GcalViewModel();
    }

    private void CalendarView_Loaded(object sender, RoutedEventArgs e)
    {
        // Scroll to the current time on load
        var currentOffset = (DateTime.Now.TimeOfDay.TotalMinutes / 1440) * ScrollViewer.ScrollableHeight;
        ScrollViewer.ScrollToVerticalOffset(currentOffset);
        
        // Periodically scroll to the current time
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(60) // Refresh every minute
        };
        timer.Tick += (s, args) =>
        {
            var currentOffset = (DateTime.Now.TimeOfDay.TotalMinutes / 1440) * ScrollViewer.ScrollableHeight;
            ScrollViewer.ScrollToVerticalOffset(currentOffset);
        };
        timer.Start();
        
        // Load the Google Calendar events asynchronously
        (DataContext as GcalViewModel).LoadGcalEventsAsync().ConfigureAwait(false);
    }
}
