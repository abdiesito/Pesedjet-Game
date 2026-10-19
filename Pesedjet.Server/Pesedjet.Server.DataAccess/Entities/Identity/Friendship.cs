using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pesedjet.Server.DataAccess.Entities.Enums;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("Friendship", Schema = "Identity")]
public class Friendship
{
    public Guid PlayerRequesterId { get; set; }
    [ForeignKey("PlayerRequesterId")]
    public Player PlayerRequester { get; set; }

    public Guid PlayerAddresseeId { get; set; }
    [ForeignKey("PlayerAddresseeId")]
    public Player PlayerAddressee { get; set; }

    [Required]
    public FriendshipStatus Status { get; set; }
}