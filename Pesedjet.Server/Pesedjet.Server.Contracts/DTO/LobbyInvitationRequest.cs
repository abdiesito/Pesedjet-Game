using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class LobbyInvitationRequest
{
    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public string LobbyCode { get; set; } = string.Empty;

    [DataMember]
    public List<Guid> TargetIds { get; set; } = new();
}
