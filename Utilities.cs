using System.Diagnostics;
using System.Windows;

namespace AgendaDashboard;

internal static class Utilities
{
    internal static async Task NotifExAsync(Func<Task> asyncFunc, string successMessage)
    {
        var notifMgr = (Application.Current as App).NotifMgr;
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

    internal class TimestampConsoleTraceListener(bool useErrorStream) : ConsoleTraceListener(useErrorStream)
    {
        private string GetTimestamp() => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ";

        public override void Write(string message)
        {
            base.Write(GetTimestamp() + message);
        }

        public override void WriteLine(string message)
        {
            base.WriteLine(GetTimestamp() + message);
        }
    }

    internal class TimestampTextWriterTraceListener(string fileName) : TextWriterTraceListener(fileName)
    {
        private string GetTimestamp() => $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ";

        public override void Write(string message)
        {
            base.Write(GetTimestamp() + message);
        }

        public override void WriteLine(string message)
        {
            base.WriteLine(GetTimestamp() + message);
        }
    }
}