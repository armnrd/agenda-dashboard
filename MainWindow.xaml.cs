using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
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
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded; // Subscribe to the Loaded event to load events when the window is ready
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Load settings from settings.json
        var settings = JsonDocument.Parse(File.ReadAllText("settings.json"));

        // Get the initial window position
        var positionElement = settings.RootElement.GetProperty("position");
        this.Left = positionElement.GetProperty("x").GetInt32() - 10; // Adjust for the title bar width
        this.Top = positionElement.GetProperty("y").GetInt32() - 4; // Adjust for the title bar height

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