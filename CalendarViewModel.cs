using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using vCard.Net.CardComponents;
using vCard.Net.Serialization;
using YamlDotNet.RepresentationModel;

namespace AgendaDashboard;

public class CalendarViewModel : INotifyPropertyChanged
{
    public List<GcalEvent> GcalEvents { get; set; } = [];
    public List<string> DateMessages { get; set; } = [];
    public DateTime TargetDate;
    private readonly IEnumerable<string?>? _selectedIds;
    private CalendarService? _serviceGcal;
    private readonly HttpClient _clientCardDav;
    private readonly string _urlCardDav;

    public CalendarViewModel()
    {
        // Initialize the target date to today
        TargetDate = DateTime.Now.Date;

        // Get Google Calendar settings from ConfigMgr
        var configGcal = (Application.Current as App).ConfigMgr.Config["google calendar"];
        var refreshIntervalGCal = double.Parse(((YamlScalarNode)configGcal["refresh interval"]).Value);
        _selectedIds = ((YamlSequenceNode)configGcal["selected ids"]).Children
            .OfType<YamlScalarNode>()
            .Select(node => node.Value);

        // Get CardDAV settings from ConfigMgr
        var configCardDav = (Application.Current as App).ConfigMgr.Config["carddav"];
        var refreshIntervalCardDav = double.Parse(((YamlScalarNode)configCardDav["refresh interval"]).Value);
        _urlCardDav = ((YamlScalarNode)configCardDav["url"]).Value;

        // Set up CardDAV client
        var credentials = JsonDocument.Parse(File.ReadAllText("credentials_carddav.json"));
        var usernameCardDav = credentials.RootElement.GetProperty("username").GetString();
        var passwordCardDav = credentials.RootElement.GetProperty("password").GetString();
        _clientCardDav = new HttpClient();
        var byteArray = Encoding.ASCII.GetBytes($"{usernameCardDav}:{passwordCardDav}");
        _clientCardDav.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        // Set up Google Calendar API service
        Task.Run(async () =>
        {
            string[] scopes = [CalendarService.Scope.CalendarReadonly];
            const string applicationName = "Agenda Dashboard";
            UserCredential credential;

            await using (var stream = new FileStream("credentials_gcal.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("gcal_token", true));
            }

            _serviceGcal = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
        });

        // Set up a timer to refresh the Google Calendar events model
        var timerGCal = new DispatcherTimer { Interval = TimeSpan.FromSeconds(refreshIntervalGCal) };
        timerGCal.Tick += async (s, e) =>
        {
            ResetTargetDate(); // Reset target date to today before loading events
            await Utilities.NotifExAsync(LoadGcalEventsAsync, "Loaded Google Calendar events.");
        };
        timerGCal.Start();

        // Set up a timer to refresh CardDAV events
        var timerCardDav = new DispatcherTimer { Interval = TimeSpan.FromSeconds(refreshIntervalCardDav) };
        timerCardDav.Tick += async (s, e) =>
        {
            await Utilities.NotifExAsync(LoadCardDavEventsAsync, "Loaded CardDAV dates.");
        };
        timerCardDav.Start();

        // Set a timer to wait for App.NotifMgr and _serviceGCal to become available and immediately call RefreshAsync()
        // var initTimer = new System.Timers.Timer(500);
        var initTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        initTimer.Tick += async (s, e) =>
        {
            if ((Application.Current as App).NotifMgr != null && _serviceGcal != null)
            {
                initTimer.Stop();
                await RefreshAsync();
            }
        };
        initTimer.Start();
    }

    internal async Task RefreshAsync()
    {
        await Task.WhenAll(
            Utilities.NotifExAsync(LoadGcalEventsAsync, "Loaded Google Calendar events."),
            Utilities.NotifExAsync(LoadCardDavEventsAsync, "Loaded CardDAV dates."));
    }

    internal void DecrementTargetDate()
    {
        TargetDate = TargetDate.AddDays(-1);
    }

    internal void ResetTargetDate()
    {
        TargetDate = DateTime.Now.Date;
    }

    internal void IncrementTargetDate()
    {
        TargetDate = TargetDate.AddDays(1);
    }

    private async Task LoadGcalEventsAsync()
    {
        var calendarList = await _serviceGcal.CalendarList.List().ExecuteAsync();
        var gcalEventsNew = new List<GcalEvent>();
        foreach (var calendar in calendarList.Items)
        {
            // Skip calendars that are not selected
            if (!_selectedIds.Contains(calendar.Id))
                continue;

            // Convert calendar hex color to Brush
            var color = (Color)ColorConverter.ConvertFromString(calendar.BackgroundColor);
            var brush = new SolidColorBrush(color);

            // Define parameters of request
            var request = _serviceGcal.Events.List(calendar.Id);
            request.TimeMin = TargetDate;
            request.TimeMax = TargetDate.AddDays(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events
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

        // Replace model and notify property change on Dispatcher
        App.Current.Dispatcher.Invoke(() =>
        {
            GcalEvents = gcalEventsNew;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(GcalEvents)));
        });
    }

    private async Task LoadCardDavEventsAsync()
    {
        // CardDAV REPORT request body to get all vCards
        string reportXml = @"<?xml version='1.0' encoding='UTF-8'?>" +
                           "<card:addressbook-query xmlns:card='urn:ietf:params:xml:ns:carddav'>" +
                           "  <dav:prop xmlns:dav='DAV:'>" +
                           "    <dav:getetag/>" +
                           "    <card:address-data/>" +
                           "  </dav:prop>" +
                           "</card:addressbook-query>";

        var request = new HttpRequestMessage(new HttpMethod("REPORT"), _urlCardDav)
        {
            Content = new StringContent(reportXml, Encoding.UTF8, "application/xml")
        };
        request.Headers.Add("Depth", "1");

        // Send request
        var response = await _clientCardDav.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseXml = await response.Content.ReadAsStringAsync();

        // Parse vCard data from response
        var xdoc = XDocument.Parse(responseXml);
        XNamespace cardns = "urn:ietf:params:xml:ns:carddav";
        var vCardStrs = xdoc.Descendants(cardns + "address-data").Select(x => x.Value);
        var vCardStrsCombined = string.Join("\n", vCardStrs);
        var tr = new StringReader(vCardStrsCombined);
        var vCards = SimpleDeserializer.Default.Deserialize(tr); // Parse vCard string

        var dateMessagesNew = new List<string>();
        foreach (VCard vCard in vCards)
        {
            if (!DateTime.TryParseExact(vCard.Birthdate, ["yyyyMMdd", "yyyy-MM-dd"], null, DateTimeStyles.None,
                    out var bd))
                continue;

            // Check if birth date in vCard matches TargetDate
            if (bd.Month == TargetDate.Month && bd.Day == TargetDate.Day)
            {
                dateMessagesNew.Add(
                    $"{vCard.FormattedName}'s {YearDiffToOrdinal(bd, TargetDate)} birthday: {bd.ToShortDateString()}");
            }
        }

        // Replace model and notify property change
        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            DateMessages = dateMessagesNew;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(DateMessages)));
        });
    }

    private static string YearDiffToOrdinal(DateTime start, DateTime end)
    {
        int years = end.Year - start.Year;
        if (end.Month < start.Month || (end.Month == start.Month && end.Day < start.Day))
            years--;

        int rem100 = years % 100;
        if (rem100 >= 11 && rem100 <= 13)
            return $"{years}th";

        switch (years % 10)
        {
            case 1: return $"{years}st";
            case 2: return $"{years}nd";
            case 3: return $"{years}rd";
            default: return $"{years}th";
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class GcalEvent
{
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string CalendarName { get; set; }
    public Brush CalendarColor { get; set; }
}