using System.Diagnostics;

namespace UI.Services;

public static class DebugLogger
{
    private static readonly string logPath = Path.Combine(AppContext.BaseDirectory, "debug-output.txt");
    private static readonly object fileLock = new();

    public static void WriteLine(string message)
    {
        var timestamped = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

        // Write to Output window
        Debug.WriteLine(timestamped);

        // Write to file
        lock (fileLock)
        {
            File.AppendAllText(logPath, timestamped + Environment.NewLine);
        }
    }
}