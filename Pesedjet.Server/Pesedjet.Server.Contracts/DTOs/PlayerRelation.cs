using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public enum PlayerRelation
{
    [EnumMember]
    None,

    [EnumMember]
    RequestSent,

    [EnumMember]
    Friends,

    [EnumMember]
    Blocked
}
