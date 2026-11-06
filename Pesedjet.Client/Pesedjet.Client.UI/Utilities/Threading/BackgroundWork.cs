using System.Threading;

namespace Pesedjet.Client.UI.Utilities.Threading;

public static class BackgroundWork
{
    public static void Start<TState, TResult>(BackgroundJob<TState, TResult> job)
    {
        ThreadPool.QueueUserWorkItem(job.Execute);
    }

    public static void Start<TState>(BackgroundAction<TState> action)
    {
        ThreadPool.QueueUserWorkItem(action.Execute);
    }
}
