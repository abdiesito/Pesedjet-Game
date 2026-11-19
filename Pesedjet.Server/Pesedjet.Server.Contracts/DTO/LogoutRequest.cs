using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class LogoutRequest
{
    [DataMember]
    public string SessionToken { get; set; } = string.Empty;

    [DataMember]
    public int PlayerId { get; set; }
}