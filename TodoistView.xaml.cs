using System.Windows;
using System.Windows.Controls;

namespace AgendaDashboard;

public partial class TodoistView : UserControl
{
    public TodoistView()
    {
        InitializeComponent();
        Loaded += TodoistView_Loaded; // Subscribe to the Loaded event to load events when the window is ready
        DataContext = new TodoistViewModel();
    }

    private void TodoistView_Loaded(object sender, RoutedEventArgs e)
    {
        // Load events when the user control is loaded
        var viewModel = (TodoistViewModel)DataContext;
        viewModel.LoadTodoistTasksAsync().ConfigureAwait(false);
    }
}
