using System.Diagnostics;
using System.Windows;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public NotifMgr NotifMgr { get; private set; }
    public ConfigMgr ConfigMgr { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up logging
        #if DEBUG
        Trace.Listeners.Add(new Utilities.TimestampConsoleTraceListener(true));
        #else
        Trace.Listeners.Add(new Utilities.TimestampTextWriterTraceListener($"log_{DateTime.Now:yyyyMMdd}.txt"));
        #endif
        Trace.AutoFlush = true;

        // Create the main window, config and notification managers
        ConfigMgr = new ConfigMgr();
        var mainWindow = new MainWindow();
        NotifMgr = new NotifMgr(mainWindow.ShowNotification);
        mainWindow.Show();
    }
}
