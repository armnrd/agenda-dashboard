using System.ComponentModel;

namespace AgendaDashboard.ViewModels;

public class DesignerSampleCalendarViewModel : INotifyPropertyChanged
{
    public List<GcalEvent> GcalEvents { get; set; } = [];
    public List<string> DateLines { get; set; } = [];

    // for (int i = 1; i <= 10; i++)
    // {
    //     GcalEvents.Add(new GcalEvent
    //     {
    //         Title = $"Sample Event {i}",
    //         Start = DateTime.Now.Date.AddHours(i),
    //         End = DateTime.Now.Date.AddHours(i + 1),
    //         CalendarName = "Sample Calendar",
    //         CalendarColor = Brushes.BlueViolet
    //     });
    // }
    //
    // DateLines.Add($"{DateTime.Now:D}");
    // for (int i = 1; i <= 4; i++)
    // {
    //     DateLines.Add($"Sample Line {i}");
    // }
    //
    // // Fire "property changed" events
    // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GcalEvents)));
    // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DateLines)));

    public event PropertyChangedEventHandler? PropertyChanged;
}
