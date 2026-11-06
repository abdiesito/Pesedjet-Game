#nullable enable

using Pesedjet.Server.Contracts.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Matchmaking.Lobbies;

public sealed class LobbyState
{
    private readonly List<LobbyConnection> _connections = new();
    private readonly Dictionary<Guid, Guid> _invitations = new();

    public LobbyState(LobbySettings settings)
    {
        LobbyCode = settings.LobbyCode;
        HostPlayerId = settings.HostPlayerId;
        IsPublic = settings.IsPublic;
        MaxPlayers = settings.MaxPlayers;
        CreatedAtUtc = settings.CreatedAtUtc;
        Status = LobbyStatus.Waiting;
    }

    public string LobbyCode { get; }

    public Guid HostPlayerId { get; private set; }

    public bool IsPublic { get; }

    public int MaxPlayers { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public LobbyStatus Status { get; private set; }

    public bool IsEmpty => _connections.Count == 0;

    public bool IsFull => _connections.Count >= MaxPlayers;

    public bool Contains(Guid playerId)
    {
        bool contains = _connections.Exists(connection => connection.Member.PlayerId == playerId);

        return contains;
    }

    public void AddOrReplace(LobbyConnection connection)
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

        // The role passes to the participant who has been longest in the lobby (CU-014 FA-04).
        if (removedCount > 0 && playerId == HostPlayerId && _connections.Count > 0)
        {
            HostPlayerId = _connections[0].Member.PlayerId;
        }

        return removedCount > 0;
    }

    public List<Guid> RemoveSession(string sessionId)
    {
        List<Guid> removedIds = _connections
            .Where(connection => connection.SessionId == sessionId)
            .Select(connection => connection.Member.PlayerId)
            .ToList();

        foreach (Guid playerId in removedIds)
        {
            Remove(playerId);
        }

        return removedIds;
    }

    public bool Invite(Guid targetPlayerId, Guid senderPlayerId)
    {
        bool isInvited = _invitations.TryAdd(targetPlayerId, senderPlayerId);

        return isInvited;
    }

    public bool CancelInvitation(Guid targetPlayerId)
    {
        bool isCancelled = _invitations.Remove(targetPlayerId);

        return isCancelled;
    }

    public bool IsInvited(Guid playerId)
    {
        bool isInvited = _invitations.ContainsKey(playerId);

        return isInvited;
    }

    public List<Guid> GetInvitedPlayerIds()
    {
        List<Guid> invitedIds = _invitations.Keys.ToList();

        return invitedIds;
    }

    public LobbyOverview ToOverview()
    {
        LobbyOverview overview = new(
            LobbyCode,
            HostPlayerId,
            MaxPlayers,
            Status == LobbyStatus.Waiting,
            GetMembersExcept(Guid.Empty),
            new Dictionary<Guid, Guid>(_invitations));

        return overview;
    }

    public void StartMatch()
    {
        Status = LobbyStatus.InGame;
    }

    public List<LobbyMember> GetMembersExcept(Guid playerId)
    {
        List<LobbyMember> members = _connections
            .Where(connection => connection.Member.PlayerId != playerId)
            .Select(connection => connection.Member)
            .ToList();

        return members;
    }

    public List<ILobbyCallback> GetCallbacksExcept(Guid playerId)
    {
        List<ILobbyCallback> callbacks = _connections
            .Where(connection => connection.Member.PlayerId != playerId)
            .Select(connection => connection.Callback)
            .ToList();

        return callbacks;
    }

    public LobbySummary ToSummary()
    {
        LobbyConnection? hostConnection = _connections.Find(
            connection => connection.Member.PlayerId == HostPlayerId);

        LobbySummary summary = new()
        {
            LobbyCode = LobbyCode,
            HostUsername = hostConnection?.Member.Username ?? string.Empty,
            CurrentPlayers = _connections.Count,
            MaxPlayers = MaxPlayers
        };

        return summary;
    }
}