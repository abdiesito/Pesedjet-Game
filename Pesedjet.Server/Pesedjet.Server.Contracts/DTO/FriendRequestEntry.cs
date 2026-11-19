using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class FriendRequestEntry
{
    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public string SenderUsername { get; set; } = string.Empty;
}
