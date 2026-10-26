using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class ServiceFault
{
    [DataMember]
    public string ErrorCode { get; set; } = string.Empty;

    [DataMember]
    public string MessageKey { get; set; } = string.Empty;
}
 