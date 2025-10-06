using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AgendaDashboard.ViewModels;
using YamlDotNet.RepresentationModel;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Set the initial window position from settings
        var config = App.Current.ConfigMgr.Config;
        Left = double.Parse(((YamlScalarNode)config["general"]["x position"]).Value) - 4; // Offset by 4px because of the title bar
        Top = double.Parse(((YamlScalarNode)config["general"]["y position"]).Value) - 4; // Same here

        var hwnd = new WindowInteropHelper(this).Handle;
        
        // Make the window a tool window: doesn't show up in taskbar or alt-tab switcher
        const int GWLP_EXSTYLE = -20; // Extended window styles
        const int WS_EX_TOOLWINDOW = 0x00000080, WS_EX_APPWINDOW = 0x00040000;
        int exStyle = GetWindowLongPtr(hwnd, GWLP_EXSTYLE);
        // Add TOOLWINDOW, remove APPWINDOW from the extended styles
        exStyle |= WS_EX_TOOLWINDOW;
        exStyle &= ~WS_EX_APPWINDOW;
        SetWindowLongPtr(hwnd, GWLP_EXSTYLE, exStyle);
    }

    internal void ShowNotification(string message, string? status)
    {
        StatusBarMessage.Text = message;
        StatusBarStatusItem.Content = status;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect the view models from CalendarView and TodoistView to TitleBar
        TitleBar.CalendarViewModel = GcalView.DataContext as CalendarViewModel;
        TitleBar.TodoistViewModel = TodoistView.DataContext as TodoistViewModel;

        // Initialize status bar
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0";
        ShowNotification($"Agenda Dashboard v{version}", "Ready"); // Show version in status bar

        // Add status bar text scrolling 
        StatusBarMessage.SizeChanged +=
            (s, e) => // Use SizeChanged event to listen for changes to the status text 
            {
                if (StatusBarMessage.ActualWidth <= StatusBarMessageItem.ActualWidth)
                {
                    // If StatusBarMessageItem is at least as wide as StatusBarMessage, don't scroll
                    StatusBarMessageTransform.BeginAnimation(TranslateTransform.XProperty, null);
                    StatusBarMessageTransform.X = 0;
                    return;
                }

                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = -StatusBarMessage.ActualWidth,
                    // Make the StatusBarMessage.Text scroll right to left

                    Duration = TimeSpan.FromSeconds(StatusBarMessage.ActualWidth * 0.01), // 1 second per 100 pixels
                    RepeatBehavior = RepeatBehavior.Forever
                };
                // StatusBarMessageTransform.BeginAnimation(TranslateTransform.XProperty,
                // animation); // TODO: fix this - status message is truncated
            };
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);
}