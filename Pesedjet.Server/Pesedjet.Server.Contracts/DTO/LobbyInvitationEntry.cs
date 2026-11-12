using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class LobbyInvitationEntry
{
    [DataMember]
    public string LobbyCode { get; set; } = string.Empty;

    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public string SenderUsername { get; set; } = string.Empty;

    [DataMember]
    public int CurrentPlayers { get; set; }

    [DataMember]
    public int MaxPlayers { get; set; }
}
