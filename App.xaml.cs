using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using AgendaDashboard.Managers;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public NotifMgr NotifMgr { get; private set; }
    public ConfigMgr ConfigMgr { get; private set; }
    public new static App Current => Application.Current as App;

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
        mainWindow.Loaded += SetUpGlobalShortcut;
        mainWindow.Show();
    }

    private void SetUpGlobalShortcut(object sender, RoutedEventArgs routedEventArgs)
    {
        var mainWindow = sender as MainWindow;
        // Set ctrl+win+# as a global keybind that temporarily raises mainWindow to the top of the z-order
        var raiseKeybind = new GlobalKeybind(
            mainWindow,
            Key.OemQuestion,
            ModifierKeys.Control | ModifierKeys.Windows,
            10000);
        raiseKeybind.Pressed += mainWindow.ToggleRaise;
    }
}