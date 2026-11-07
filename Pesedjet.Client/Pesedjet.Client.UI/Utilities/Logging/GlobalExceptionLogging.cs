using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Pesedjet.Client.UI.Utilities.Logging;

// Last line of defence: logs the exceptions that no catch block handled, on any thread.
public static class GlobalExceptionLogging
{
    private const string AppDomainSource = "AppDomain.UnhandledException";
    private const string TaskSource = "TaskScheduler.UnobservedTaskException";
    private const string UiThreadSource = "Dispatcher.UnhandledException";
    private const string UnknownExceptionMessage = "An unhandled exception without a managed exception object was raised.";

    // Call before Avalonia starts. Covers worker threads; the process still ends after an AppDomain exception.
    public static void AttachToProcess()
    {
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    // Call once the UI thread exists. An exception raised from a UI callback is logged and the app keeps running.
    public static void AttachToUiThread()
    {
        Dispatcher.UIThread.UnhandledException += OnUiThreadException;
    }

    private static void OnAppDomainException(object sender, UnhandledExceptionEventArgs eventArgs)
    {
        if (eventArgs.ExceptionObject is Exception exception)
        {
            ApplicationLog.Error(AppDomainSource, exception);
        }
        else
        {
            ApplicationLog.Warning(AppDomainSource, UnknownExceptionMessage);
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs eventArgs)
    {
        ApplicationLog.Error(TaskSource, eventArgs.Exception);
        eventArgs.SetObserved();
    }

    private static void OnUiThreadException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
    {
        ApplicationLog.Error(UiThreadSource, eventArgs.Exception);
        eventArgs.Handled = true;
    }
}
