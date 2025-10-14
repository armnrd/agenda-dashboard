using System.Windows;
using System.Windows.Threading;

namespace AgendaDashboard.Managers;

public class NotifMgr
{
    private readonly Action<string, string> _showMsgAction;
    private readonly DispatcherTimer _msgTimer;
    // (message, status)
    private readonly Queue<(string message, string status)> _msgQueue;
    private bool _statusBarEmpty;

    public NotifMgr(Action<string, string> showMsgAction)
    {
        _showMsgAction = showMsgAction ?? throw new ArgumentNullException(nameof(showMsgAction),
            "Notification action cannot be null.");

        // Start a timer that shows messages every two seconds
        _msgTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _msgTimer.Tick += (_, _) => ShowNextMessage();
        _msgTimer.Start();

        _msgQueue = [];
        _statusBarEmpty = true;
    }

    public void QueueMessage(string message, string status)
    {
        _msgQueue.Enqueue((message, status));
        if (_statusBarEmpty) // Show the message immediately if the status bar is empty
        {
            Application.Current.Dispatcher.InvokeAsync(ShowNextMessage, DispatcherPriority.Normal);
            _statusBarEmpty = false;
            // Reset the timer
            _msgTimer.Stop();
            _msgTimer.Start();
        }
    }

    private void ShowNextMessage()
    {
        if (_msgQueue.Count == 0)
        {
            // Queue a "ready" status message
            _msgQueue.Enqueue(("", "Ready"));
            _statusBarEmpty = true;
        }

        var (message, status) = _msgQueue.Dequeue();
        _showMsgAction(message, status); // Show the message
    }
}