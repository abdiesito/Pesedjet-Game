using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class FriendRequestEntry
{
    [DataMember]
    public Guid SenderId { get; set; }

    [DataMember]
    public string SenderUsername { get; set; } = string.Empty;
}
