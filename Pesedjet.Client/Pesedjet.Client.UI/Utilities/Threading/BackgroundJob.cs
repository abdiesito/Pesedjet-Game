using System;
using System.Diagnostics;
using Avalonia.Threading;
using Pesedjet.Client.UI.Utilities.Logging;

namespace Pesedjet.Client.UI.Utilities.Threading;

public sealed class BackgroundJob<TState, TResult>
{
    private const string FailureSource = "BackgroundJob";

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
    
    public void Cancel()
    {
        IsCancelled = true;
    }
    
    public void Execute(object? unused)
    {
        try
        {
            _result = Work(State);
        }
        catch (Exception exception)
        {
            _error = exception;
            ApplicationLog.Error(FailureSource, exception);
        }

        Dispatcher.UIThread.Post(Complete);
    }
    
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
