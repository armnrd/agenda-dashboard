using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.Configuration;

namespace AgendaDashboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public GcalViewModel GcalViewModel { get; } = new GcalViewModel();
    public TodoistViewModel TodoistViewModel { get; }= new TodoistViewModel();
    
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded; // Subscribe to the Loaded event to load events when the window is ready
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
            .Build();
        
        this.Left = config.GetSection("position").GetValue<int>("x");
        this.Top = config.GetSection("position").GetValue<int>("y");
        
        // var hwnd = new WindowInteropHelper(this).Handle;
        // int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        // exStyle |= WS_EX_TOOLWINDOW;
        // SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
    }

    // const int GWL_EXSTYLE = -20;
    // const int WS_EX_TOOLWINDOW = 0x00000080;

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);
    
}