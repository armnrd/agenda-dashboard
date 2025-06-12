using System.Windows;
using System.Windows.Controls;

namespace AgendaDashboard;

public partial class TodoistView : UserControl
{
    public TodoistView()
    {
        InitializeComponent();
        DataContext = new TodoistViewModel();
    }
}
