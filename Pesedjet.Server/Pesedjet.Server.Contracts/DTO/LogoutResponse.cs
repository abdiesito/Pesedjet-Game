namespace Pesedjet.Server.Contracts.DTOs;

using System.Runtime.Serialization;

[DataContract]
public class LogoutResponse
{
    [DataMember]
    public bool IsSuccess { get; set; }

    [DataMember]
    public string ErrorCode { get; set; } = string.Empty;
}