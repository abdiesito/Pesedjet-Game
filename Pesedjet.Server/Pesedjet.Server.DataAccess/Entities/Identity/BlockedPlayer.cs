using System.ComponentModel.DataAnnotations.Schema;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("BlockedPlayer", Schema = "Identity")]
public class BlockedPlayer
{
    public Guid PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public Player Player { get; set; } = null!;

    public Guid BlockedId { get; set; }

    [ForeignKey(nameof(BlockedId))]
    public Player Blocked { get; set; } = null!;
}
