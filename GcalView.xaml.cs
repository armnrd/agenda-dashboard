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

        // Scroll to the current time line every 60 seconds
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        timer.Tick += (s, args) =>
        {
            ScrollViewer.ScrollToVerticalOffset((DateTime.Now.TimeOfDay.TotalMinutes / 1440) *
                                                ScrollViewer.ScrollableHeight); // 1440 minutes in a day
        };
        timer.Start();
    }
    
    private void PreviousDayButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as GcalViewModel;
        viewModel.DecrementTargetDate(); 
        viewModel.SafeLoadGcalEvents();
    }
    
    private void CurrentDayButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as GcalViewModel;
        viewModel.ResetTargetDate(); 
        viewModel.SafeLoadGcalEvents();
    }
    
    private void NextDayButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as GcalViewModel;
        viewModel.IncrementTargetDate(); 
        viewModel.SafeLoadGcalEvents();
    }
    
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        (DataContext as GcalViewModel)?.SafeLoadGcalEvents();
    }
}