using Pesedjet.Client.Networking.Results;

namespace Pesedjet.Client.Networking.Interfaces;

public interface IServiceCaller
{
    ClientResult<TResult> Call<TChannel, TArgument, TResult>(
        string address,
        Func<TChannel, TArgument, TResult> operation,
        TArgument argument)
        where TChannel : class;
}
