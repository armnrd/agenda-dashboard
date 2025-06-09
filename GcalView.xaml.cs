using System.Windows;
using System.Windows.Controls;

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
        // Load events when the user control is loaded
        var viewModel = (GcalViewModel)DataContext;
        viewModel.LoadGoogleCalendarEventsAsync().ConfigureAwait(false);
    }
}
