using Pesedjet.Server.Contracts.Internal;
using CoreWCF;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Friendships;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class FriendshipNotificationService : IFriendshipNotificationService
{
    private readonly IFriendshipNotifier _notifier;

    public FriendshipNotificationService(IFriendshipNotifier notifier)
    {
        _notifier = notifier;
    }

    public bool Subscribe(Guid playerId)
    {
        if (playerId == Guid.Empty)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        IFriendshipCallback callback = OperationContext.Current.GetCallbackChannel<IFriendshipCallback>();
        IContextChannel channel = (IContextChannel)callback;
        string sessionId = OperationContext.Current.SessionId ?? string.Empty;

        _notifier.Subscribe(playerId, callback, sessionId);

        channel.Closed -= OnChannelClosed;
        channel.Faulted -= OnChannelClosed;
        channel.Closed += OnChannelClosed;
        channel.Faulted += OnChannelClosed;

        return true;
    }

    public void Unsubscribe(Guid playerId)
    {
        _notifier.Unsubscribe(playerId);
    }

    private void OnChannelClosed(object? sender, EventArgs eventArgs)
    {
        string sessionId = (sender as IContextChannel)?.SessionId ?? string.Empty;

        _notifier.UnsubscribeSession(sessionId);
    }

    private static FaultException<ServiceFault> CreateFault(string errorCode, string messageKey)
    {
        ServiceFault fault = new ServiceFault
        {
            ErrorCode = errorCode,
            MessageKey = messageKey
        };

        return new FaultException<ServiceFault>(fault, new FaultReason(errorCode));
    }
}
