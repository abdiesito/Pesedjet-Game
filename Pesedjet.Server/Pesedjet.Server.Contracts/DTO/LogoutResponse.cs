using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class LogoutResponse
{
    [DataMember]
    public bool IsSuccess { get; set; }

    [DataMember]
    public string ErrorCode { get; set; } = string.Empty;
}