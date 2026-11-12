using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs
{
    [DataContract]
    public class UserSession
    {
        [DataMember]
        public Guid PlayerId { get; set; }

        [DataMember] 
        public string Username { get; set; } = string.Empty;
        
        [DataMember] 
        public string PreferredLanguage { get; set; } = string.Empty;
        
        [DataMember] 
        public string RememberToken { get; set; } = string.Empty;
        
        [DataMember] 
        public bool IsGuest { get; set; }
    }
}