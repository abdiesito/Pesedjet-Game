using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class PlayerSearchRequest
{
    [DataMember]
    public Guid RequesterId { get; set; }

    [DataMember]
    public string SearchTerm { get; set; } = string.Empty;
}
