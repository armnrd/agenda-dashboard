using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using AgendaDashboard.ViewModels;

namespace AgendaDashboard.Controls;

public partial class TitleBar : UserControl
{
    public CalendarViewModel CalendarViewModel { get; set; }
    public TodoistViewModel TodoistViewModel { get; set; }

    public TitleBar()
    {
        InitializeComponent();
        Loaded += TitleBar_Loaded; // Subscribe to the Loaded event to initialize the window chrome
    }

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        // Lock the window by default; do this by checking the "Lock Window" menu item
        foreach (var item in MainMenu.Items)
            if ((item as MenuItem).Header?.ToString() == "Lock Window")
            {
                (item as MenuItem).IsChecked = true;
                break;
            }
    }

    private void MainMenuButton_Checked(object sender, RoutedEventArgs e)
    {
        MainMenu.PlacementTarget = MainMenuButton;
        MainMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        MainMenu.IsOpen = true;
        MainMenu.HorizontalOffset = MainMenu.ActualWidth - MainMenuButton.ActualWidth;
    }
    
    private void MainMenuButton_Unchecked(object sender, RoutedEventArgs e)
    {
        MainMenu.IsOpen = false;
    }
    
    private void MainMenu_Closed(object sender, RoutedEventArgs e)
    {
        MainMenuButton.IsChecked = false;
    }

    private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CalendarViewModel.Refresh();
        TodoistViewModel.Refresh();
    }

    private void LockMenuItem_Checked(object sender, RoutedEventArgs e)
    {
        // Disable dragging and resizing
        WindowChrome.SetWindowChrome(Window.GetWindow(this), new WindowChrome
        {
            CaptionHeight = 0, // Prevent dragging by setting the caption height to 0
            ResizeBorderThickness = new Thickness(0) // Prevent resizing by setting the resize border thickness to 0
        });
    }

    private void LockMenuItem_Unchecked(object sender, RoutedEventArgs e)
    {
        // Enable dragging and resizing
        WindowChrome.SetWindowChrome(Window.GetWindow(this), new WindowChrome
        {
            CaptionHeight = ActualHeight, // Restore the caption height to allow dragging
            ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness // Restore the resize border thickness
        });
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        App.Current.Shutdown();
    }
}