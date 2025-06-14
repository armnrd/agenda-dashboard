using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace AgendaDashboard;

public class TodoistTask
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime? DueDate { get; set; }
    public bool Checked { get; set; }
    public short DayOrder { get; set; }
}

public class TodoistViewModel : INotifyPropertyChanged
{
    private Timer _timer;
    private string _apiToken;

    public ObservableCollection<TodoistTask> TodoistTasks { get; set; } = new();

    public TodoistViewModel()
    {
        // Extract credentials from todoist_credentials.json
        var credentials = JsonDocument.Parse(File.ReadAllText("todoist_credentials.json"));
        _apiToken = credentials.RootElement.GetProperty("api-token").GetString();

        // Load settings from settings.json
        var settings = JsonDocument.Parse(File.ReadAllText("settings.json"));

        // Get the refresh interval
        var refreshInterval = settings.RootElement
            .GetProperty("todoist")
            .GetProperty("refresh-interval").GetInt32();

        // Set up a timer to refresh the tasks model
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(refreshInterval) };
        timer.Tick += async (s, e) => await LoadTodoistTasksAsync();
        timer.Start();

        // Load tasks immediately on startup
        _ = LoadTodoistTasksAsync();
    }

    public async Task LoadTodoistTasksAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

        var response = await client.GetAsync("https://api.todoist.com/api/v1/tasks/filter?query=today");
        response.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        // Get an array enumerator for the results element in the JSON response
        var tasksEnumerator = json.RootElement.GetProperty("results").EnumerateArray();
        
        // Create a new list to hold the TodoistTask objects
        var todoistTasksNew = new List<TodoistTask>();
        foreach (var task in tasksEnumerator)
        {
            todoistTasksNew.Add(new TodoistTask()
            {
                Id = task.GetProperty("id").GetString(),
                Content = task.GetProperty("content").GetString(),
                Checked = task.GetProperty("checked").GetBoolean(),
                DueDate = DateTime.Parse(task.GetProperty("due").GetProperty("date").GetString()),
                DayOrder = task.GetProperty("day_order").GetInt16()
            });
        }
        
        // Sort the tasks by day order
        todoistTasksNew.Sort((x, y) => x.DayOrder.CompareTo(y.DayOrder));

        // Update the TodoistTasks collection from within the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            TodoistTasks.Clear();
            // Populate TodoistTasks with the new tasks
            foreach (var todoistTask in todoistTasksNew)
                TodoistTasks.Add(todoistTask);
        });
    }


    private class TodoistTaskDto
    {
        public string id { get; set; }
        public string content { get; set; }
        public string[] labels { get; set; }
        public bool is_completed { get; set; }
        public DueDto due { get; set; }
    }

    private class DueDto
    {
        public string date { get; set; }
    }

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public event PropertyChangedEventHandler PropertyChanged;
}