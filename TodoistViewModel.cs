using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AgendaDashboard;


public class TaskItem
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
}

public class TodoistViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TaskItem> TodaysTasks { get; set; } = new();
    
    public async Task LoadTodoistTasksAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("todoist_credentials.json", optional: false, reloadOnChange: true)
            .Build();
        var apiToken = config.GetValue<string>("api-token");
        
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var response = await client.GetAsync("https://api.todoist.com/rest/v2/tasks");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var allTasks = JsonSerializer.Deserialize<List<TodoistTaskDto>>(json);

        var today = DateTime.Today;
        TodaysTasks.Clear();
        foreach (var task in allTasks)
        {
            if (task.due?.date == today.ToString("yyyy-MM-dd"))
            {
                TodaysTasks.Add(new TaskItem()
                {
                    Id = task.id,
                    Content = task.content,
                    IsCompleted = task.is_completed,
                    DueDate = DateTime.Parse(task.due.date)
                });
            }
        }

        var tasks = TodaysTasks;
        return;
    }
    
    
    private class TodoistTaskDto
    {
        public string id { get; set; }
        public string content { get; set; }
        public bool is_completed { get; set; }
        public DueDto due { get; set; }
    }

    private class DueDto
    {
        public string date { get; set; }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
}
