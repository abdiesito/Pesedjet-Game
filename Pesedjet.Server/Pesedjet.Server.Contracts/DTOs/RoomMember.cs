#nullable enable

using System;
using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class RoomMember
{
    [DataMember]
    public Guid PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; } = string.Empty;

    [DataMember]
    public bool IsGuest { get; set; }
}