using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class InvitableFriendsRequest
{
    [DataMember]
    public Guid PlayerId { get; set; }

    [DataMember]
    public string LobbyCode { get; set; } = string.Empty;
}
