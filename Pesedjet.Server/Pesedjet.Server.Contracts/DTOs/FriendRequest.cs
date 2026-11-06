using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class FriendRequest
{
    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public Guid TargetId { get; set; }
}
