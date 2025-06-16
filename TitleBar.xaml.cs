using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace AgendaDashboard;

public partial class TitleBar : UserControl
{
    public GcalViewModel GcalViewModel { get; set; }
    public TodoistViewModel TodoistViewModel { get; set; }

    private WindowChrome _mainWindowChromeUnlocked, _mainWindowChromeLocked;

    public TitleBar()
    {
        InitializeComponent();
        Loaded += TitleBar_Loaded; // Subscribe to the Loaded event to initialize the window chrome
    }

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        // Define the custom unlocked window chrome for the main window
        _mainWindowChromeUnlocked =
            new WindowChrome
            {
                CaptionHeight = DockPanel.Height, // Use the height of this title bar as height of the caption area
                CornerRadius = new CornerRadius(0), // No corner radius
                GlassFrameThickness = new Thickness(0) // No glass frame thickness
            };

        // Define the custom locked window chrome to prevent dragging and resizing
        _mainWindowChromeLocked =
            new WindowChrome
            {
                CaptionHeight = 0, // Set caption height to 0 to prevent dragging
                ResizeBorderThickness = new Thickness(0), // Set resize border thickness to 0 to prevent resizing
                CornerRadius = new CornerRadius(0), // No corner radius
                GlassFrameThickness = new Thickness(0) // No glass frame thickness
            };

        // Set the custom locked chrome for the main window by default
        // Do this by checking the "Lock Window" menu item
        foreach (var item in MainMenu.Items)
            if ((item as MenuItem).Header?.ToString() == "Lock Window")
            {
                (item as MenuItem).IsChecked = true;
                break;
            }
    }

    private void MainMenuButton_Click(object sender, RoutedEventArgs e)
    {
        MainMenuButton.ContextMenu.PlacementTarget = MainMenuButton;
        MainMenuButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        MainMenuButton.ContextMenu.IsOpen = true;
    }

    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            GcalViewModel?.LoadGcalEventsAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Show an error message if loading fails
            ((MainWindow)Application.Current.MainWindow).ShowNotification(
                $"Error loading Google Calendar events: {ex.Message}", "Error");
            // Log the exception to Trace
            System.Diagnostics.Trace.WriteLine(
                $"{DateTime.Now:HH:mm:ss} - Error loading Google Calendar events: {ex.Message}");
        }

        try
        {
            TodoistViewModel?.LoadTodoistTasksAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Show an error message if loading fails
            ((MainWindow)Application.Current.MainWindow).ShowNotification($"Error loading Todoist tasks: {ex.Message}",
                "Error");
            // Log the exception to Trace
            System.Diagnostics.Trace.WriteLine(
                $"{DateTime.Now:HH:mm:ss} - Error loading Todoist tasks: {ex.Message}");
        }
    }

    private void LockMenuItem_Checked(object sender, RoutedEventArgs e)
    {
        // Prevent dragging and resizing by setting a locked window chrome
        WindowChrome.SetWindowChrome(Window.GetWindow(this), _mainWindowChromeLocked);
    }

    private void LockMenuItem_Unchecked(object sender, RoutedEventArgs e)
    {
        // Allow dragging and resizing by restoring the original window chrome
        WindowChrome.SetWindowChrome(Window.GetWindow(this), _mainWindowChromeUnlocked);
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}