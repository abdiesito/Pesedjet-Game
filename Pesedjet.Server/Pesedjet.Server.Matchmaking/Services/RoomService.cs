#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CoreWCF;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.Matchmaking.Rooms;

namespace Pesedjet.Server.Matchmaking.Services;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class RoomService : IRoomService
{
    private const string RoomCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int RoomCodeLength = 6;
    private const int MinimumMaxPlayers = 2;
    private const int MaximumMaxPlayers = 6;

    private readonly Dictionary<string, RoomState> _rooms = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _roomLock = new();
    private readonly TimeProvider _timeProvider;

    public RoomService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public string CreateRoom(RoomMember host, bool isPublic, int maxPlayers)
    {
        string roomCode = string.Empty;
        ServiceFault? fault = null;

        if (maxPlayers < MinimumMaxPlayers || maxPlayers > MaximumMaxPlayers)
        {
            fault = CreateFault(ServiceErrorCodes.InvalidRoomCapacity, ServiceMessageKeys.InvalidRoomCapacity);
        }
        else
        {
            RoomConnection hostConnection = new(host, GetCurrentCallback());
            DateTimeOffset createdAtUtc = _timeProvider.GetUtcNow();

            lock (_roomLock)
            {
                roomCode = GenerateUniqueRoomCode();
                RoomState room = new(roomCode, host.PlayerId, isPublic, maxPlayers, createdAtUtc);
                room.AddOrReplace(hostConnection);
                _rooms.Add(roomCode, room);
            }
        }

        ThrowIfFaulted(fault);

        return roomCode;
    }

    public Task<RoomJoinResponse> JoinRoomAsync(string roomCode, RoomMember player)
    {
        RoomConnection connection = new(player, GetCurrentCallback());
        string normalizedCode = roomCode?.Trim() ?? string.Empty;
        List<IRoomCallback> callbacksToNotify = new();
        Task<RoomJoinResponse> result;

        lock (_roomLock)
        {
            if (!_rooms.TryGetValue(normalizedCode, out RoomState? room))
            {
                result = CreateFaultedTask<RoomJoinResponse>(
                    ServiceErrorCodes.RoomNotFound,
                    ServiceMessageKeys.RoomNotFound);
            }
            else if (room.Status != RoomStatus.Waiting)
            {
                result = CreateFaultedTask<RoomJoinResponse>(
                    ServiceErrorCodes.RoomAlreadyStarted,
                    ServiceMessageKeys.RoomAlreadyStarted);
            }
            else if (room.Contains(player.PlayerId))
            {
                room.AddOrReplace(connection);
                result = Task.FromResult(CreateJoinResponse(room, player.PlayerId));
            }
            else if (room.IsFull)
            {
                result = CreateFaultedTask<RoomJoinResponse>(
                    ServiceErrorCodes.RoomFull,
                    ServiceMessageKeys.RoomFull);
            }
            else
            {
                RoomJoinResponse response = CreateJoinResponse(room, player.PlayerId);
                callbacksToNotify = room.GetCallbacksExcept(player.PlayerId);
                room.AddOrReplace(connection);
                result = Task.FromResult(response);
            }
        }

        Notify(callbacksToNotify, callback => callback.PlayerJoined(player));

        return result;
    }

    public void LeaveRoom(string roomCode, Guid playerId)
    {
        string normalizedCode = roomCode?.Trim() ?? string.Empty;
        List<IRoomCallback> callbacksToNotify = new();
        ServiceFault? fault = null;

        lock (_roomLock)
        {
            if (!_rooms.TryGetValue(normalizedCode, out RoomState? room))
            {
                fault = CreateFault(ServiceErrorCodes.RoomNotFound, ServiceMessageKeys.RoomNotFound);
            }
            else if (room.Remove(playerId))
            {
                if (room.IsEmpty)
                {
                    _rooms.Remove(room.RoomCode);
                }
                else
                {
                    callbacksToNotify = room.GetCallbacksExcept(playerId);
                }
            }
        }

        Notify(callbacksToNotify, callback => callback.PlayerLeft(playerId));
        ThrowIfFaulted(fault);
    }

    public List<RoomSummary> GetPublicRooms()
    {
        List<RoomSummary> publicRooms;

        lock (_roomLock)
        {
            publicRooms = _rooms.Values
                .Where(room => room.IsPublic && room.Status == RoomStatus.Waiting && !room.IsFull)
                .OrderBy(room => room.CreatedAtUtc)
                .Select(room => room.ToSummary())
                .ToList();
        }

        return publicRooms;
    }

    private string GenerateUniqueRoomCode()
    {
        string roomCode;

        do
        {
            roomCode = RandomNumberGenerator.GetString(RoomCodeAlphabet, RoomCodeLength);
        }
        while (_rooms.ContainsKey(roomCode));

        return roomCode;
    }

    private static RoomJoinResponse CreateJoinResponse(RoomState room, Guid joiningPlayerId)
    {
        RoomJoinResponse response = new()
        {
            RoomCode = room.RoomCode,
            HostPlayerId = room.HostPlayerId,
            MaxPlayers = room.MaxPlayers,
            ExistingMembers = room.GetMembersExcept(joiningPlayerId)
        };

        return response;
    }

    private static IRoomCallback GetCurrentCallback()
    {
        IRoomCallback callback = OperationContext.Current.GetCallbackChannel<IRoomCallback>();

        return callback;
    }

    private static void Notify(IReadOnlyList<IRoomCallback> callbacks, Action<IRoomCallback> notification)
    {
        foreach (IRoomCallback callback in callbacks)
        {
            try
            {
                notification(callback);
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

    private static Task<TResult> CreateFaultedTask<TResult>(string errorCode, string messageKey)
    {
        FaultException<ServiceFault> exception = new(CreateFault(errorCode, messageKey));

        return Task.FromException<TResult>(exception);
    }

    private static void ThrowIfFaulted(ServiceFault? fault)
    {
        if (fault is not null)
        {
            throw new FaultException<ServiceFault>(fault);
        }
    }

    private static ServiceFault CreateFault(string errorCode, string messageKey)
    {
        ServiceFault fault = new()
        {
            ErrorCode = errorCode,
            MessageKey = messageKey
        };

        return fault;
    }
}