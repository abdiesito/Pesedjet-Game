using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class FriendRequest
{
    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public Guid TargetId { get; set; }
}
