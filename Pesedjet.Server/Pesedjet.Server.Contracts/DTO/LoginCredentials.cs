using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO
{
    [DataContract]
    public class LoginCredentials
    {
        [DataMember] 
        public string Username { get; set; } = string.Empty;
        
        [DataMember] 
        public string Password { get; set; } = string.Empty;
        
        
        [DataMember]
        public string DeviceToken { get; set; } = string.Empty;
    }
}