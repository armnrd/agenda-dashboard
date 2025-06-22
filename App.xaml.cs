using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up logging
        Trace.Listeners.Add(new TextWriterTraceListener($"log_{DateTime.Now:yyyyMMdd}.txt"));
        Trace.AutoFlush = true;
    }
}
