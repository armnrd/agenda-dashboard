using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded; // Subscribe to the Loaded event to load events when the window is ready
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect the view models from GcalView and TodoistView to TitleBar
        TitleBar.GcalViewModel = GcalView.DataContext as GcalViewModel;
        TitleBar.TodoistViewModel = TodoistView.DataContext as TodoistViewModel;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Load settings from settings.json
        var settings = JsonDocument.Parse(File.ReadAllText("settings.json"));

        // Get the initial window position
        var positionElement = settings.RootElement.GetProperty("position");
        this.Left = positionElement.GetProperty("x").GetInt32() - 4; // Offset by 4px because of the title bar
        this.Top = positionElement.GetProperty("y").GetInt32() - 4; // Same here
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy,
        uint uFlags);
}