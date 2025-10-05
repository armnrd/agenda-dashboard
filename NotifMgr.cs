using System.Windows;
using System.Windows.Threading;

namespace AgendaDashboard;

public class NotifMgr(Action<string, string> _notifAction)
{
    private readonly Queue<(string message, string status, double duration)> _queue = [];

    private bool _isShowing;

    // (message, status)
    private readonly Action<string, string> _action = _notifAction ?? throw new ArgumentNullException(
        nameof(_notifAction), "Notification action cannot be null.");

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
            Application.Current.Dispatcher.InvokeAsync(() => _action("", "Ready")); // Clear status bar
            _isShowing = false;
            return;
        }

        var (message, status, duration) = _queue.Dequeue();
        Application.Current.Dispatcher.InvokeAsync(() => _action(message, status)); // Show the message
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