#nullable enable

using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class RoomSummary
{
    [DataMember]
    public string RoomCode { get; set; } = string.Empty;

    [DataMember]
    public string HostUsername { get; set; } = string.Empty;

    [DataMember]
    public int CurrentPlayers { get; set; }

    [DataMember]
    public int MaxPlayers { get; set; }
}