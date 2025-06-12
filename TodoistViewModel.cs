using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;

namespace AgendaDashboard;

public class TodoistTask
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
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

        // Set up a timer to refresh tasks
        _timer = new Timer(async _ => await LoadTodoistTasksAsync(), null, 0, refreshInterval * 1000);
    }

    public async Task LoadTodoistTasksAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

        var response = await client.GetAsync("https://api.todoist.com/rest/v2/tasks");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var allTasks = JsonSerializer.Deserialize<List<TodoistTaskDto>>(json);

        var today = DateTime.Today;

        var todoistTasksNew = new List<TodoistTask>();
        foreach (var task in allTasks)
        {
            if (task.due?.date == today.ToString("yyyy-MM-dd"))
            {
                todoistTasksNew.Add(new TodoistTask()
                {
                    Id = task.id,
                    Content = task.content,
                    IsCompleted = task.is_completed,
                    DueDate = DateTime.Parse(task.due.date)
                });
            }
        }

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