using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using YamlDotNet.RepresentationModel;

namespace AgendaDashboard;

public class TodoistViewModel : INotifyPropertyChanged
{
    public List<TodoistTask> TodoistTasks { get; private set; } = [];
    private readonly HttpClient _client;
    private readonly string? _query;

    public TodoistViewModel()
    {
        // Get settings from ConfigMgr TODO: error handling
        var config = (Application.Current as App).ConfigMgr.Config["todoist"];
        var refreshInterval = double.Parse(((YamlScalarNode)config["refresh interval"]).Value);
        _query = ((YamlScalarNode)config["query"]).Value;

        // Set up client
        var credentials = JsonDocument.Parse(File.ReadAllText("credentials_todoist.json"));
        var apiToken = credentials.RootElement.GetProperty("api-token").GetString();
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        // Set up a timer to refresh the tasks model
        var timer = new System.Timers.Timer(refreshInterval * 1000);
        timer.Elapsed += async (s, e) => { await RefreshAsync(); };
        timer.Start();

        // Set a timer to wait for App.NotifMgr to become available and immediately call RefreshAsync()
        var initTimer = new System.Timers.Timer(500);
        initTimer.Elapsed += async (s, e) =>
        {
            if ((Application.Current as App).NotifMgr != null)
            {
                initTimer.Stop();
                await RefreshAsync();
            }
        };
        initTimer.Start();
    }

    internal async Task RefreshAsync()
    {
        await Utilities.NotifExAsync(LoadTodoistTasksAsync, "Loaded Todoist tasks.");
    }

    private async Task LoadTodoistTasksAsync()
    {
        var response = await _client.GetAsync($"https://api.todoist.com/api/v1/tasks/filter?query={_query}");
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
                DayOrder = task.GetProperty("day_order").GetInt16(),
                ChildOrder = task.GetProperty("child_order").GetInt16()
            });
        }

        // Sort the tasks by day order
        todoistTasksNew.Sort((x, y) =>
        {
            // If either task has a DayOrder of -1, it should be sorted to the end
            if (x.DayOrder == -1)
                return 1;
            if (y.DayOrder == -1)
                return -1;

            return x.DayOrder.CompareTo(y.DayOrder);
        });

        // Replace model and notify property change on Dispatcher
        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            TodoistTasks = todoistTasksNew;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(TodoistTasks)));
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class TodoistTask
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime? DueDate { get; set; }
    public bool Checked { get; set; }
    public short DayOrder { get; set; }
    public short ChildOrder { get; set; }
}