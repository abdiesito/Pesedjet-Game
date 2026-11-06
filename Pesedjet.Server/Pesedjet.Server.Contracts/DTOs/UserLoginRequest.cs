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
        
        // Token this machine received on its last login; empty when the machine has none.
        [DataMember]
        public string DeviceToken { get; set; } = string.Empty;
    }
}