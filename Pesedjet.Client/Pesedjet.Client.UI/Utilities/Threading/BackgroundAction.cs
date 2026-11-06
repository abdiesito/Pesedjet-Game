using System;
using Pesedjet.Client.UI.Utilities.Logging;

namespace Pesedjet.Client.UI.Utilities.Threading;

// A call on a worker thread whose outcome nobody waits for (for example a one-way operation).
// A failure is only written to the application log because the caller has nothing left to do with it.
public sealed class BackgroundAction<TState>
{
    private const string FailureSource = "BackgroundAction";

    public required Action<TState> Work { get; init; }

    public required TState State { get; init; }

    // Runs on the worker thread (WaitCallback signature).
    public void Execute(object? unused)
    {
        try
        {
            Work(State);
        }
        catch (Exception exception)
        {
            ApplicationLog.Error(FailureSource, exception);
        }
    }
}
