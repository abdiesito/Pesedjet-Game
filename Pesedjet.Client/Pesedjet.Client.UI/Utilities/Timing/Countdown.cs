using System;
using System.Threading;
using Avalonia.Threading;

namespace Pesedjet.Client.UI.Utilities.Timing;

public sealed class Countdown : IDisposable
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly TimeProvider _timeProvider;
    private readonly Action<int> _onTick;
    private readonly Action _onFinished;

    private ITimer? _timer;
    private int _remainingSeconds;

    public int RemainingSeconds => _remainingSeconds;

    public Countdown(TimeProvider timeProvider, Action<int> onTick, Action onFinished)
    {
        _timeProvider = timeProvider;
        _onTick = onTick;
        _onFinished = onFinished;
    }

    public void Start(int seconds)
    {
        Stop();

        _remainingSeconds = seconds;
        _onTick(_remainingSeconds);
        _timer = _timeProvider.CreateTimer(HandleTimer, null, Interval, Interval);
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        Stop();
    }

    private void HandleTimer(object? state)
    {
        Dispatcher.UIThread.Post(Tick);
    }

    private void Tick()
    {
        if (_timer is not null)
        {
            _remainingSeconds--;
            _onTick(_remainingSeconds);

            if (_remainingSeconds <= 0)
            {
                Stop();
                _onFinished();
            }
        }
    }
}
