#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Matchmaking.Rooms;

public sealed class RoomState
{
    private readonly List<RoomConnection> _connections = new();

    public RoomState(
        string roomCode,
        Guid hostPlayerId,
        bool isPublic,
        int maxPlayers,
        DateTimeOffset createdAtUtc)
    {
        RoomCode = roomCode;
        HostPlayerId = hostPlayerId;
        IsPublic = isPublic;
        MaxPlayers = maxPlayers;
        CreatedAtUtc = createdAtUtc;
        Status = RoomStatus.Waiting;
    }

    public string RoomCode { get; }

    public Guid HostPlayerId { get; }

    public bool IsPublic { get; }

    public int MaxPlayers { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public RoomStatus Status { get; private set; }

    public bool IsEmpty => _connections.Count == 0;

    public bool IsFull => _connections.Count >= MaxPlayers;

    public bool Contains(Guid playerId)
    {
        bool contains = _connections.Exists(connection => connection.Member.PlayerId == playerId);

        return contains;
    }

    public void AddOrReplace(RoomConnection connection)
    {
        int index = _connections.FindIndex(existing => existing.Member.PlayerId == connection.Member.PlayerId);
        if (index >= 0)
        {
            _connections[index] = connection;
        }
        else
        {
            _connections.Add(connection);
        }
    }

    public bool Remove(Guid playerId)
    {
        int removedCount = _connections.RemoveAll(connection => connection.Member.PlayerId == playerId);

        return removedCount > 0;
    }

    public void StartMatch()
    {
        Status = RoomStatus.InGame;
    }

    public List<RoomMember> GetMembersExcept(Guid playerId)
    {
        List<RoomMember> members = _connections
            .Where(connection => connection.Member.PlayerId != playerId)
            .Select(connection => connection.Member)
            .ToList();

        return members;
    }

    public List<IRoomCallback> GetCallbacksExcept(Guid playerId)
    {
        List<IRoomCallback> callbacks = _connections
            .Where(connection => connection.Member.PlayerId != playerId)
            .Select(connection => connection.Callback)
            .ToList();

        return callbacks;
    }

    public RoomSummary ToSummary()
    {
        RoomConnection? hostConnection = _connections.Find(
            connection => connection.Member.PlayerId == HostPlayerId);

        RoomSummary summary = new()
        {
            RoomCode = RoomCode,
            HostUsername = hostConnection?.Member.Username ?? string.Empty,
            CurrentPlayers = _connections.Count,
            MaxPlayers = MaxPlayers
        };

        return summary;
    }
}