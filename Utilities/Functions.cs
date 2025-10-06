using System.Diagnostics;

namespace AgendaDashboard.Utilities;

internal static partial class Functions
{
    internal static async Task NotifExAsync(Func<Task> asyncFunc, string successMessage)
    {
        var notifMgr = App.Current.NotifMgr;
        try
        {
            await asyncFunc();
        }
        catch (Exception ex)
        {
            // Show an error message if loading fails
            notifMgr.Enqueue($"{asyncFunc.Method.Name}(): {ex.Message}", "Error", 5);
            Trace.WriteLine($"{asyncFunc.Method.Name}(): {ex.Message}");
            return;
        }

        // Successful, show success message
        notifMgr.Enqueue(successMessage, "Success", 2);
    }
}