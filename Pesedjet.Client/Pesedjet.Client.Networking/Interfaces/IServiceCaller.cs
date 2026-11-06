using Pesedjet.Client.Networking.Results;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IServiceCaller
{
    // Runs one request-reply operation on a short-lived channel and turns every possible failure into a ClientResult.
    // The call blocks: invoke it from a worker thread, never from the UI thread.
    ClientResult<TResult> Call<TChannel, TArgument, TResult>(
        string address,
        Func<TChannel, TArgument, TResult> operation,
        TArgument argument)
        where TChannel : class;
}
