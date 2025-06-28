using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public NotificationManager NM;

    public class NotificationManager(Action<string, string> notificationAction)
    {
        private readonly Queue<(string message, string status, double duration)> _queue = new();
        private bool _isShowing;

        private readonly Action<string, string> _notificationAction = notificationAction ??
                                                                      throw new ArgumentNullException(
                                                                          nameof(notificationAction),
                                                                          "Notification action cannot be null");

        public void Enqueue(string message, string status, double duration = 2)
        {
            _queue.Enqueue((message, status, duration));
            if (!_isShowing)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _notificationAction?.Invoke("", "Ready"); // Clear status bar
                _isShowing = false;
                return;
            }

            var (message, status, duration) = _queue.Dequeue();
            _notificationAction.Invoke(message, status);
            _isShowing = true;

            // Start a timer to show the next notification after duration seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ShowNext(); // Show the next notification in the queue
            };
            timer.Start();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        NM = new NotificationManager(ShowNotification);
        Loaded += MainWindow_Loaded; // Subscribe to the Loaded event to load events when the window is ready
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect the view models from GcalView and TodoistView to TitleBar
        TitleBar.GcalViewModel = GcalView.DataContext as GcalViewModel;
        TitleBar.TodoistViewModel = TodoistView.DataContext as TodoistViewModel;

        // Initialize status bar
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0";
        NM.Enqueue($"Agenda Dashboard v{version}", "Ready", 5); // Show version in status bar

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

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Load settings from settings.json
        var settings = JsonDocument.Parse(File.ReadAllText("settings.json"));

        // Get the initial window position from settings
        var positionElement = settings.RootElement.GetProperty("position");
        // Set the initial position of the window
        this.Left = positionElement.GetProperty("x").GetInt32() - 4; // Offset by 4px because of the title bar
        this.Top = positionElement.GetProperty("y").GetInt32() - 4; // Same here

        // Make the window a tool window (no taskbar button, no alt-tab)
        var hwnd = new WindowInteropHelper(this).Handle;
        int exStyle = GetWindowLong(hwnd, -20); // GWL_EXSTYLE = -20
        SetWindowLong(hwnd, -20, exStyle | 0x00000080); // WS_EX_TOOLWINDOW = 0x00000080
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy,
        uint uFlags);

    private void ShowNotification(string message, string? status)
    {
        StatusBarMessage.Text = message;
        StatusBarStatusItem.Content = status;
    }
}