using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AgendaDashboard;

public partial class CalendarView : UserControl
{
    public CalendarView()
    {
        InitializeComponent();
        DataContext = new CalendarViewModel();
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
        var viewModel = DataContext as CalendarViewModel;
        viewModel.DecrementTargetDate();
        viewModel.RefreshAsync();
    }
    
    private void CurrentDayButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as CalendarViewModel;
        viewModel.ResetTargetDate();
        viewModel.RefreshAsync();
    }
    
    private void NextDayButton_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as CalendarViewModel;
        viewModel.IncrementTargetDate();
        viewModel.RefreshAsync();
    }
    
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        (DataContext as CalendarViewModel).RefreshAsync();
    }
}