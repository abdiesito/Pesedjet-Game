using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class RespondFriendRequest
{
    [DataMember]
    public Guid ResponderId { get; set; }

    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public bool Accept { get; set; }
}
