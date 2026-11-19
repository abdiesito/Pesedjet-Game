using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class FriendEntry
{
    [DataMember]
    public Guid PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; } = string.Empty;
}
