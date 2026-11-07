using System;
using Pesedjet.Client.UI.Utilities.Logging;

namespace Pesedjet.Client.UI.Utilities.Threading;

public sealed class BackgroundAction<TState>
{
    private const string FailureSource = "BackgroundAction";

    public required Action<TState> Work { get; init; }

    public required TState State { get; init; }
    
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
