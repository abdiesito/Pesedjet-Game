namespace Pesedjet.Server.Contracts.DTOs;

using System.Runtime.Serialization;

[DataContract]
public class LogoutRequest
{
    [DataMember]
    public string SessionToken { get; set; } = string.Empty;

    [DataMember]
    public int PlayerId { get; set; }
}