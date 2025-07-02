using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace AgendaDashboard;

public class GcalEvent
{
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string CalendarName { get; set; }
    public System.Windows.Media.Brush CalendarColor { get; set; }
}

public class GcalViewModel : INotifyPropertyChanged
{
    private List<string> _ignoredIds;
    private DateTime _targetDate;

    public ObservableCollection<GcalEvent> GcalEvents { get; set; } = new();

    public GcalViewModel()
    {
        // Load settings from settings.json
        var settings = JsonDocument.Parse(File.ReadAllText("settings.json"));

        // Set the target date to today
        _targetDate = DateTime.Now.Date;
        
        // Get the list of ignored calendar IDs
        var ignoredIdsElement = settings.RootElement
            .GetProperty("google-calendar")
            .GetProperty("ignored-ids");
        _ignoredIds = ignoredIdsElement.EnumerateArray()
            .Select(e => e.GetString())
            .ToList();

        // Get the refresh interval
        var refreshInterval = settings.RootElement
            .GetProperty("google-calendar")
            .GetProperty("refresh-interval").GetInt32();

        // Set up a timer to refresh the events model
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(refreshInterval) };
        timer.Tick += async (s, e) => { SafeLoadGcalEvents(); };
        timer.Start();

        // Load events immediately on startup
        SafeLoadGcalEvents();
    }
    
    public void DecrementTargetDate()
    {
        _targetDate = _targetDate.AddDays(-1);
    }
    
    public void ResetTargetDate()
    {
        _targetDate = DateTime.Now.Date;
    }

    public void IncrementTargetDate()
    {
        _targetDate = _targetDate.AddDays(1);
    }
    
    public void SafeLoadGcalEvents()
    {
        LoadGcalEventsAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                // Show an error message if loading fails
                (Application.Current.MainWindow as MainWindow).NM.Enqueue(
                    $"GCal: {t.Exception.Message}", "Error", 5);
                // Log the exception to Trace
                System.Diagnostics.Trace.WriteLine(
                    $"{DateTime.Now:HH:mm:ss} - GCal: {t.Exception.Message}");
            }
            else
            {
                // Successfully loaded events, show a success message
                (Application.Current.MainWindow as MainWindow).NM.Enqueue("Loaded Google Calendar events.",
                    "Success", 2);
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public async Task LoadGcalEventsAsync()
    {
        string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        string ApplicationName = "AgendaDashboard";
        UserCredential credential;

        using (var stream = new FileStream("gcal_credentials.json", FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("gcal_token", true));
        }

        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        var calendarList = await service.CalendarList.List().ExecuteAsync();
        var gcalEventsNew = new List<GcalEvent>();
        foreach (var calendar in calendarList.Items)
        {
            // Skip calendars that are in the ignored-ids list
            if (_ignoredIds.Contains(calendar.Id))
                continue;

            // Convert calendar hex color to Brush
            var color = (Color)ColorConverter.ConvertFromString(calendar.BackgroundColor);
            var brush = new SolidColorBrush(color);

            // Define parameters of request.
            var request = service.Events.List(calendar.Id);
            request.TimeMin = _targetDate;
            request.TimeMax = _targetDate.AddDays(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            var events = await request.ExecuteAsync();

            foreach (var ev in events.Items.Where(e => e.Start.DateTime.HasValue && e.End.DateTime.HasValue))
            {
                gcalEventsNew.Add(new GcalEvent
                {
                    Title = ev.Summary,
                    Start = ev.Start.DateTime.Value,
                    End = ev.End.DateTime.Value,
                    CalendarName = calendar.Summary,
                    CalendarColor = brush
                });
            }
        }

        // Update the GcalEvents collection from within the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            GcalEvents.Clear();
            // Populate GcalEvents with the new tasks
            foreach (var gcalEvent in gcalEventsNew)
                GcalEvents.Add(gcalEvent);
        });
    }

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public event PropertyChangedEventHandler PropertyChanged;
}