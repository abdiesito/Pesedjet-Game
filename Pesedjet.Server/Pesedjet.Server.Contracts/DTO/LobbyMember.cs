#nullable enable

using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class LobbyMember
{
    [DataMember]
    public Guid PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; } = string.Empty;

    [DataMember]
    public bool IsGuest { get; set; }
}