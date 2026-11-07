using Pesedjet.Server.Contracts.Internal;
using System.Security.Cryptography;
using CoreWCF;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.Matchmaking.Lobbies;

namespace Pesedjet.Server.Matchmaking;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class LobbyService : ILobbyService, ILobbyDirectory
{
    private const string LobbyCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int LobbyCodeLength = 6;
    private const int MinimumMaxPlayers = 2;
    private const int MaximumMaxPlayers = 6;

    private readonly Dictionary<string, LobbyState> _lobbies = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lobbyLock = new();
    private readonly HashSet<string> _watchedSessions = new();
    private readonly TimeProvider _timeProvider;
    private readonly IFriendshipNotifier _notifier;

    public LobbyService(TimeProvider timeProvider, IFriendshipNotifier notifier)
    {
        _timeProvider = timeProvider;
        _notifier = notifier;
    }

    public string CreateLobby(LobbyMember host, bool isPublic, int maxPlayers)
    {
        if (maxPlayers < MinimumMaxPlayers || maxPlayers > MaximumMaxPlayers)
        {
            throw CreateFault(ServiceErrorCodes.InvalidLobbyCapacity, ServiceMessageKeys.InvalidLobbyCapacity);
        }

        LobbyConnection hostConnection = new(host, GetCurrentCallback(), WatchCurrentSession());
        DateTimeOffset createdAtUtc = _timeProvider.GetUtcNow();
        string lobbyCode;

        lock (_lobbyLock)
        {
            lobbyCode = GenerateUniqueLobbyCode();
            LobbySettings settings = new(lobbyCode, host.PlayerId, isPublic, maxPlayers, createdAtUtc);
            LobbyState lobby = new(settings);
            lobby.AddOrReplace(hostConnection);
            _lobbies.Add(lobbyCode, lobby);
        }

        return lobbyCode;
    }

    public LobbyJoinResponse JoinLobby(string lobbyCode, LobbyMember player)
    {
        LobbyConnection connection = new(player, GetCurrentCallback(), WatchCurrentSession());
        string normalizedCode = lobbyCode?.Trim() ?? string.Empty;
        List<ILobbyCallback> callbacksToNotify = new();
        LobbyJoinResponse response;

        lock (_lobbyLock)
        {
            if (!_lobbies.TryGetValue(normalizedCode, out LobbyState? lobby))
            {
                throw CreateFault(ServiceErrorCodes.LobbyNotFound, ServiceMessageKeys.LobbyNotFound);
            }

            if (lobby.Status != LobbyStatus.Waiting)
            {
                throw CreateFault(ServiceErrorCodes.LobbyAlreadyStarted, ServiceMessageKeys.LobbyAlreadyStarted);
            }

            if (lobby.Contains(player.PlayerId))
            {
                lobby.CancelInvitation(player.PlayerId);
                lobby.AddOrReplace(connection);
                response = CreateJoinResponse(lobby, player.PlayerId);
            }
            else if (lobby.IsFull)
            {
                throw CreateFault(ServiceErrorCodes.LobbyFull, ServiceMessageKeys.LobbyFull);
            }
            else
            {
                response = CreateJoinResponse(lobby, player.PlayerId);
                callbacksToNotify = lobby.GetCallbacksExcept(player.PlayerId);
                lobby.CancelInvitation(player.PlayerId);
                lobby.AddOrReplace(connection);
            }
        }

        Notify(callbacksToNotify, SendPlayerJoined, player);

        return response;
    }

    // One-way operation: it cannot return a fault, so leaving an unknown lobby is ignored.
    public void LeaveLobby(string lobbyCode, Guid playerId)
    {
        string normalizedCode = lobbyCode?.Trim() ?? string.Empty;
        List<ILobbyCallback> callbacksToNotify = new();
        List<Guid> withdrawnInvitedIds = new();

        lock (_lobbyLock)
        {
            if (_lobbies.TryGetValue(normalizedCode, out LobbyState? lobby) && lobby.Remove(playerId))
            {
                if (lobby.IsEmpty)
                {
                    withdrawnInvitedIds = lobby.GetInvitedPlayerIds();
                    _lobbies.Remove(lobby.LobbyCode);
                }
                else
                {
                    callbacksToNotify = lobby.GetCallbacksExcept(playerId);
                }
            }
        }

        Notify(callbacksToNotify, SendPlayerLeft, playerId);
        WithdrawInvitations(normalizedCode, withdrawnInvitedIds);
    }

    public List<LobbySummary> GetPublicLobbies()
    {
        List<LobbySummary> publicLobbies;

        lock (_lobbyLock)
        {
            publicLobbies = _lobbies.Values
                .Where(lobby => lobby.IsPublic && lobby.Status == LobbyStatus.Waiting && !lobby.IsFull)
                .OrderBy(lobby => lobby.CreatedAtUtc)
                .Select(lobby => lobby.ToSummary())
                .ToList();
        }

        return publicLobbies;
    }

    public LobbyOverview? FindLobby(string lobbyCode)
    {
        string normalizedCode = lobbyCode?.Trim() ?? string.Empty;
        LobbyOverview? overview = null;

        lock (_lobbyLock)
        {
            if (_lobbies.TryGetValue(normalizedCode, out LobbyState? lobby))
            {
                overview = lobby.ToOverview();
            }
        }

        return overview;
    }

    public bool IsInAnyLobby(Guid playerId)
    {
        bool isInLobby;

        lock (_lobbyLock)
        {
            isInLobby = _lobbies.Values.Any(lobby => lobby.Contains(playerId));
        }

        return isInLobby;
    }

    public bool RegisterInvitation(string lobbyCode, Guid targetPlayerId, Guid senderPlayerId)
    {
        string normalizedCode = lobbyCode?.Trim() ?? string.Empty;
        bool isRegistered = false;

        lock (_lobbyLock)
        {
            if (_lobbies.TryGetValue(normalizedCode, out LobbyState? lobby)
                && lobby.Status == LobbyStatus.Waiting
                && !lobby.Contains(targetPlayerId))
            {
                isRegistered = lobby.Invite(targetPlayerId, senderPlayerId);
            }
        }

        return isRegistered;
    }

    public bool CancelInvitation(string lobbyCode, Guid targetPlayerId)
    {
        string normalizedCode = lobbyCode?.Trim() ?? string.Empty;
        bool isCancelled = false;

        lock (_lobbyLock)
        {
            if (_lobbies.TryGetValue(normalizedCode, out LobbyState? lobby))
            {
                isCancelled = lobby.CancelInvitation(targetPlayerId);
            }
        }

        return isCancelled;
    }

    public List<LobbyOverview> FindLobbiesInviting(Guid playerId)
    {
        List<LobbyOverview> overviews;

        lock (_lobbyLock)
        {
            overviews = _lobbies.Values
                .Where(lobby => lobby.Status == LobbyStatus.Waiting && lobby.IsInvited(playerId))
                .Select(lobby => lobby.ToOverview())
                .ToList();
        }

        return overviews;
    }

    // A closed or faulted channel means the client left or dropped: its players are removed from every lobby.
    private string WatchCurrentSession()
    {
        IContextChannel channel = (IContextChannel)GetCurrentCallback();
        string sessionId = OperationContext.Current.SessionId ?? string.Empty;
        bool isNewSession;

        lock (_lobbyLock)
        {
            isNewSession = _watchedSessions.Add(sessionId);
        }

        if (isNewSession)
        {
            channel.Closed += OnChannelClosed;
            channel.Faulted += OnChannelClosed;
        }

        return sessionId;
    }

    private void OnChannelClosed(object? sender, EventArgs eventArgs)
    {
        string sessionId = (sender as IContextChannel)?.SessionId ?? string.Empty;
        List<(List<ILobbyCallback> Callbacks, List<Guid> LeftIds)> notifications = new();
        List<(string LobbyCode, List<Guid> InvitedIds)> withdrawals = new();

        lock (_lobbyLock)
        {
            _watchedSessions.Remove(sessionId);

            foreach (LobbyState lobby in _lobbies.Values.ToList())
            {
                List<Guid> leftIds = lobby.RemoveSession(sessionId);

                if (lobby.IsEmpty)
                {
                    withdrawals.Add((lobby.LobbyCode, lobby.GetInvitedPlayerIds()));
                    _lobbies.Remove(lobby.LobbyCode);
                }
                else if (leftIds.Count > 0)
                {
                    notifications.Add((lobby.GetCallbacksExcept(Guid.Empty), leftIds));
                }
            }
        }

        foreach ((List<ILobbyCallback> callbacks, List<Guid> leftIds) in notifications)
        {
            foreach (Guid leftId in leftIds)
            {
                Notify(callbacks, SendPlayerLeft, leftId);
            }
        }

        foreach ((string lobbyCode, List<Guid> invitedIds) in withdrawals)
        {
            WithdrawInvitations(lobbyCode, invitedIds);
        }
    }

    private void WithdrawInvitations(string lobbyCode, List<Guid> invitedIds)
    {
        foreach (Guid invitedId in invitedIds)
        {
            _notifier.NotifyLobbyInvitationWithdrawn(invitedId, lobbyCode);
        }
    }

    private string GenerateUniqueLobbyCode()
    {
        string lobbyCode;

        do
        {
            lobbyCode = RandomNumberGenerator.GetString(LobbyCodeAlphabet, LobbyCodeLength);
        }
        while (_lobbies.ContainsKey(lobbyCode));

        return lobbyCode;
    }

    private static LobbyJoinResponse CreateJoinResponse(LobbyState lobby, Guid joiningPlayerId)
    {
        LobbyJoinResponse response = new()
        {
            LobbyCode = lobby.LobbyCode,
            HostPlayerId = lobby.HostPlayerId,
            IsPublic = lobby.IsPublic,
            MaxPlayers = lobby.MaxPlayers,
            ExistingMembers = lobby.GetMembersExcept(joiningPlayerId)
        };

        return response;
    }

    private static ILobbyCallback GetCurrentCallback()
    {
        ILobbyCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

        return callback;
    }

    private static void SendPlayerJoined(ILobbyCallback callback, LobbyMember player)
    {
        callback.PlayerJoined(player);
    }

    private static void SendPlayerLeft(ILobbyCallback callback, Guid playerId)
    {
        callback.PlayerLeft(playerId);
    }

    private static void Notify<TArgument>(
        IReadOnlyList<ILobbyCallback> callbacks,
        Action<ILobbyCallback, TArgument> notification,
        TArgument argument)
    {
        foreach (ILobbyCallback callback in callbacks)
        {
            try
            {
                notification(callback, argument);
            }
            catch (CommunicationException)
            {
                // Unreachable clients are removed by the heartbeat disconnection flow, not by notifications.
            }
            catch (TimeoutException)
            {
                // Unreachable clients are removed by the heartbeat disconnection flow, not by notifications.
            }
            catch (ObjectDisposedException)
            {
                // Unreachable clients are removed by the heartbeat disconnection flow, not by notifications.
            }
        }
    }

    private static FaultException<ServiceFault> CreateFault(string errorCode, string messageKey)
    {
        ServiceFault fault = new()
        {
            ErrorCode = errorCode,
            MessageKey = messageKey
        };

        return new FaultException<ServiceFault>(fault, new FaultReason(errorCode));
    }
}
