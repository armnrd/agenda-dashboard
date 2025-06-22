using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public NotificationManager NM { get; set; } = new();
    
    public class NotificationManager
    {
        private readonly Queue<(string message, string status)> _queue = new();
        private bool _isShowing;
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) }; // Show notifications every 2 seconds
        private Action<string, string> _notificationAction;
    
        public NotificationManager()
        {
            _timer.Tick += (s, e) => ShowNext();
        }
    
        public void SetNotificationAction(Action<string, string> notificationAction)
        {
            _notificationAction = notificationAction;
            if (_queue.Count > 0 && !_isShowing)
                ShowNext();
        }
    
        public void Enqueue(string message, string status)
        {
            _queue.Enqueue((message, status));
            if (_notificationAction != null && !_isShowing)
                ShowNext();
        }
    
        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _notificationAction?.Invoke("", "Ready"); // Clear status bar
                _isShowing = false;
                _timer.Stop();
                return;
            }
    
            var (message, status) = _queue.Dequeue();
            _notificationAction?.Invoke(message, status);
            _isShowing = true;
            _timer.Start();
        }
    }
    
    public MainWindow()
    {
        InitializeComponent();

        // Initialize status bar
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0";
        StatusBarMessage.Text = $"Agenda Dashboard v{version}";

        // Add status bar text scrolling 
        // StatusBarMessage.SizeChanged += (s, e) => // Use SizeChanged event instead of Loaded to ensure the width is correct
        // {
        //     if (StatusBarMessageItem.ActualWidth > StatusBarMessage.ActualWidth)
        //     {
        //         // If the StatusBarMessageItem is wider than the message text, don't scroll
        //         StatusBarMessageTransform.X = 0;
        //         return;
        //     }
        //     
        //     var animation = new DoubleAnimation
        //     {
        //         From = 0,
        //         To = -StatusBarMessage.ActualWidth,
        //         Duration = TimeSpan.FromSeconds(8),
        //         RepeatBehavior = RepeatBehavior.Forever
        //     };
        //     StatusBarMessageTransform.BeginAnimation(TranslateTransform.XProperty, animation); // TODO: fix this - status message is truncated
        // };


        Loaded += MainWindow_Loaded; // Subscribe to the Loaded event to load events when the window is ready
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect the view models from GcalView and TodoistView to TitleBar
        TitleBar.GcalViewModel = GcalView.DataContext as GcalViewModel;
        TitleBar.TodoistViewModel = TodoistView.DataContext as TodoistViewModel;
        NM.SetNotificationAction(ShowNotification);
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

    public void ShowNotification(string message, string? status)
    {
        StatusBarMessage.Text = message;
        StatusBarStatusItem.Content = status;
    }
}