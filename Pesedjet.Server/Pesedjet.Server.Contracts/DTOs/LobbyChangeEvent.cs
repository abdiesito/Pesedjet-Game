using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs
{
    public enum LobbyEventType
    {
        PlayerJoined,
        PlayerLeft,
        PlayerKicked,
        SettingsUpdated,
        GameStarting
    }

    [DataContract]
    public class LobbyChangeEvent
    {
        [DataMember] 
        public LobbyEventType EventType { get; set; }
        
        [DataMember] 
        public string AffectedUsername { get; set; } = string.Empty;
        
        [DataMember] 
        public string Reason { get; set; } = string.Empty;
        
        //[DataMember] 
        //public LobbySettings UpdatedSettings { get; set; } = null!;
        
        //[DataMember] 
        //public List<LobbyPlayer> CurrentPlayers { get; set; } = new();
    }
}