using System;
using System.Diagnostics;
using System.IO;

namespace Pesedjet.Client.UI.Utilities.Logging;

// Development log of the client. Every entry goes to the debug output, to the console and to a daily file under
// the user's application data folder. Writing a log entry never throws: a log that fails must not break the app.
public static class ApplicationLog
{
    private const string AppFolderName = "Pesedjet";
    private const string LogFolderName = "logs";
    private const string LogFileNameFormat = "client-{0:yyyyMMdd}.log";
    private const string EntryFormat = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}: {3}";
    private const string ErrorLevel = "ERROR";
    private const string WarningLevel = "WARN";

    private static readonly object WriteLock = new();

    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppFolderName,
        LogFolderName);

    public static void Error(string source, Exception exception)
    {
        Write(ErrorLevel, source, exception.ToString());
    }

    public static void Warning(string source, string message)
    {
        Write(WarningLevel, source, message);
    }

    private static void Write(string level, string source, string details)
    {
        DateTime now = DateTime.Now;
        string entry = string.Format(EntryFormat, now, level, source, details);

        Trace.WriteLine(entry);
        Console.Error.WriteLine(entry);

        try
        {
            lock (WriteLock)
            {
                Directory.CreateDirectory(LogDirectory);
                string logFilePath = Path.Combine(LogDirectory, string.Format(LogFileNameFormat, now));
                File.AppendAllText(logFilePath, entry + Environment.NewLine);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"The log file could not be written: {ex.Message}");
        }
    }
}
