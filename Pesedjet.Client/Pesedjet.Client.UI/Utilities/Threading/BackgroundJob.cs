using System;
using System.Diagnostics;
using Avalonia.Threading;

namespace Pesedjet.Client.UI.Utilities.Threading;

// One blocking call that runs on a worker thread and reports back on the UI thread.
// Work runs off the UI thread and must not touch UI-bound state. OnCompleted and OnFailed run on the UI thread.
// State is handed to all three, so the callbacks are plain methods and nothing is captured implicitly.
// Cancel() cannot interrupt the blocking call; it only discards the outcome: OnCompleted and OnFailed never run.
// If a cancelled call still succeeds, OnCompletedAfterCancel (optional) lets the owner undo what the server did.
// Set a breakpoint in Execute, Complete or any of the callbacks to follow a call from start to finish.
public sealed class BackgroundJob<TState, TResult>
{
    private TResult? _result;
    private Exception? _error;

    public required Func<TState, TResult> Work { get; init; }

    public required TState State { get; init; }

    public required Action<TResult, TState> OnCompleted { get; init; }

    public required Action<Exception, TState> OnFailed { get; init; }

    public Action<TResult, TState>? OnCompletedAfterCancel { get; init; }

    public bool IsCancelled { get; private set; }

    public TResult? Result => _result;

    public Exception? Error => _error;

    // Call from the UI thread, for example when the screen that started the job is left.
    public void Cancel()
    {
        IsCancelled = true;
    }

    // Runs on the worker thread (WaitCallback signature).
    public void Execute(object? unused)
    {
        try
        {
            _result = Work(State);
        }
        catch (Exception exception)
        {
            _error = exception;
            Debug.WriteLine($"BackgroundJob failed: {exception}");
        }

        Dispatcher.UIThread.Post(Complete);
    }

    // Runs on the UI thread.
    private void Complete()
    {
        if (IsCancelled)
        {
            Discard();
        }
        else if (_error is null)
        {
            OnCompleted(_result!, State);
        }
        else
        {
            OnFailed(_error, State);
        }
    }

    private void Discard()
    {
        Debug.WriteLine($"BackgroundJob cancelled; outcome discarded (failed: {_error is not null}).");

        if (_error is null && OnCompletedAfterCancel is not null)
        {
            OnCompletedAfterCancel(_result!, State);
        }
    }
}
