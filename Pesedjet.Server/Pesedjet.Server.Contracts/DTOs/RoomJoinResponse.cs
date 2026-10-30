#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class RoomJoinResponse
{
    [DataMember]
    public string RoomCode { get; set; } = string.Empty;

    [DataMember]
    public Guid HostPlayerId { get; set; }

    [DataMember]
    public int MaxPlayers { get; set; }

    [DataMember]
    public List<RoomMember> ExistingMembers { get; set; } = new();
}