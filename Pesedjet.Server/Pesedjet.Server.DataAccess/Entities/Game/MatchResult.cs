using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pesedjet.Server.DataAccess.Entities.Enums;
using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Entities.Game;

[Table("MatchResult", Schema = "Game")]
public class MatchResult
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid MatchId { get; set; }
    [ForeignKey("MatchId")]
    public Match Match { get; set; }
    
    public Guid? PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public Player? Player { get; set; }

    [Required]
    public int Score { get; set; }

    [Required]
    public int Amulets { get; set; }

    [Required]
    public bool IsWinner { get; set; }

    [Required]
    public int Rank { get; set; }

    [Required]
    public MatchExitReason ExitReason { get; set; } 

    public ExpulsionReason? ExpulsionReason { get; set; }
}