using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace AgendaDashboard;

public partial class TitleBar : UserControl
{
    public GcalViewModel GcalViewModel { get; set; }
    public TodoistViewModel TodoistViewModel { get; set; }

    private WindowChrome _mainWindowChromeNormal, _mainWindowChromeLocked;

    public TitleBar()
    {
        InitializeComponent();
        Loaded += TitleBar_Loaded; // Subscribe to the Loaded event to initialize the window chrome
    }

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        // Define the custom normal window chrome for the main window
        _mainWindowChromeNormal =
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
        
        // Set the custom normal chrome for the main window
        WindowChrome.SetWindowChrome(Window.GetWindow(this), _mainWindowChromeNormal);
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        MenuButton.ContextMenu.PlacementTarget = MenuButton;
        MenuButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        MenuButton.ContextMenu.IsOpen = true;
    }

    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        GcalViewModel?.LoadGcalEventsAsync().ConfigureAwait(false);
        TodoistViewModel?.LoadTodoistTasksAsync().ConfigureAwait(false);
    }


    private void LockMenuItem_Checked(object sender, RoutedEventArgs e)
    {
        // Prevent dragging and resizing by setting a locked window chrome
        WindowChrome.SetWindowChrome(Window.GetWindow(this),_mainWindowChromeLocked); 
    }

    private void LockMenuItem_Unchecked(object sender, RoutedEventArgs e)
    {
        // Allow dragging and resizing by restoring the original window chrome
        WindowChrome.SetWindowChrome(Window.GetWindow(this), _mainWindowChromeNormal);
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}