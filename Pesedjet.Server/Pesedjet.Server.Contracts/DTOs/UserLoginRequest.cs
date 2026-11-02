using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs
{
    [DataContract]
    public class UserLoginRequest
    {
        [DataMember] 
        public string Username { get; set; } = string.Empty;
        
        [DataMember] 
        public string Password { get; set; } = string.Empty;
        
        [DataMember] 
        public bool RememberSession { get; set; }
    }
}