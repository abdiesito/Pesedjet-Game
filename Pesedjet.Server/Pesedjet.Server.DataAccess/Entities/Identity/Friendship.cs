using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pesedjet.Server.DataAccess.Entities.Enums;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("Friendship", Schema = "Identity")]
public class Friendship
{
    public Guid PlayerRequesterId { get; set; }

    [ForeignKey(nameof(PlayerRequesterId))]
    public Player PlayerRequester { get; set; } = null!;

    public Guid PlayerAddresseeId { get; set; }

    [ForeignKey(nameof(PlayerAddresseeId))]
    public Player PlayerAddressee { get; set; } = null!;

    [Required]
    public FriendshipStatus Status { get; set; }
}
