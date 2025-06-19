using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
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
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up logging
        Trace.Listeners.Add(new TextWriterTraceListener($"log_{DateTime.Now:yyyyMMdd}.txt"));
        Trace.AutoFlush = true;
    }
}
