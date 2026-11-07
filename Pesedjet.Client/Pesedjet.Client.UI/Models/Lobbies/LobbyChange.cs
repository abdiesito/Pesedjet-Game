using System;
using LobbyMemberDto = Pesedjet.Server.Contracts.DTOs.LobbyMember;

namespace Pesedjet.Client.UI.Models.Lobbies;

public sealed record LobbyChange(LobbyMemberDto? Joined, Guid LeftPlayerId);
