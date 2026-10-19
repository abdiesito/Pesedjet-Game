using System.ComponentModel.DataAnnotations.Schema;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("BlockedPlayer", Schema = "Identity")]
public class BlockedPlayer
{
    public Guid PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public Player Player { get; set; }

    public Guid BlockedId { get; set; }
    [ForeignKey("BlockedId")]
    public Player Blocked { get; set; }
}