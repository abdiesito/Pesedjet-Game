using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class ServiceFaultDto
{
    [DataMember] public string ErrorCode { get; set; }
    [DataMember] public string MessageKey { get; set; }
}