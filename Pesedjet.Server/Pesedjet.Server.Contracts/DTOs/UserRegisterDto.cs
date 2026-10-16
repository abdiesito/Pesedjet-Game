using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs;

[DataContract]
public class UserRegisterDto
{
    [DataMember] public string FullName { get; set; }
    [DataMember] public string Username { get; set; }
    [DataMember] public string Email { get; set; } 
    [DataMember] public string Password { get; set; } 
    [DataMember] public DateTime BirthDate  { get; set; }
    
}

