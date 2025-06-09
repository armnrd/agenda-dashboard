using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Windows.Media;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace AgendaDashboard;

public class CalendarEvent
{
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string CalendarName { get; set; }
    public System.Windows.Media.Brush CalendarColor { get; set; }
}

public class GcalViewModel
{
    public ObservableCollection<CalendarEvent> GcalEvents { get; set; } = new();

    public async Task LoadGoogleCalendarEventsAsync()
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
        foreach (var calendar in calendarList.Items)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();
            
            var ignoredIds = config.GetSection("google-calendar:ignored-ids").Get<List<string>>();
            
            // Skip calendars that are not in the ignored-ids list
            if (ignoredIds.Contains(calendar.Id))
                continue;

            // Convert calendar hex color to Brush
            var color = (Color)ColorConverter.ConvertFromString(calendar.BackgroundColor);
            var brush = new SolidColorBrush(color);

            // Define parameters of request.
            var request = service.Events.List(calendar.Id);
            request.TimeMin = DateTime.Now.Date;
            request.TimeMax = DateTime.Now.Date.AddDays(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            var events = await request.ExecuteAsync();

            foreach (var ev in events.Items.Where(e => e.Start.DateTime.HasValue && e.End.DateTime.HasValue))
            {
                GcalEvents.Add(new CalendarEvent
                {
                    Title = ev.Summary,
                    Start = ev.Start.DateTime.Value,
                    End = ev.End.DateTime.Value,
                    CalendarName = calendar.Summary,
                    CalendarColor = brush
                });
            }
        }
    }
}