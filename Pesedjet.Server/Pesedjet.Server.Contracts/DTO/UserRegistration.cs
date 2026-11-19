using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO;

[DataContract]
public class UserRegistration
{
    [DataMember]
    public string FullName { get; set; } = string.Empty;

    [DataMember]
    public string Username { get; set; } = string.Empty;

    [DataMember]
    public string Email { get; set; } = string.Empty;

    [DataMember]
    public string Password { get; set; } = string.Empty;

    [DataMember]
    public DateTime BirthDate { get; set; }
}
