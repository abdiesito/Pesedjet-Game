using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.DataAccess.Entities.Game;
using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Contexts;

public class PesedjetDatabaseContext : DbContext
{
    public DbSet<Player> Players { get; set; }

    public DbSet<VerificationCode> VerificationCodes { get; set; }

    public DbSet<Session> Sessions { get; set; }

    public DbSet<Friendship> Friendships { get; set; }

    public DbSet<BlockedPlayer> BlockedPlayers { get; set; }

    public DbSet<Match> Matches { get; set; }

    public DbSet<MatchResult> MatchResults { get; set; }

    public PesedjetDatabaseContext(DbContextOptions<PesedjetDatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Friendship>()
            .HasKey(friendship => new { friendship.PlayerRequesterId, friendship.PlayerAddresseeId });

        modelBuilder.Entity<Friendship>()
            .HasOne(friendship => friendship.PlayerRequester)
            .WithMany()
            .HasForeignKey(friendship => friendship.PlayerRequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Friendship>()
            .HasOne(friendship => friendship.PlayerAddressee)
            .WithMany()
            .HasForeignKey(friendship => friendship.PlayerAddresseeId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BlockedPlayer>()
            .HasKey(blockedPlayer => new { blockedPlayer.PlayerId, blockedPlayer.BlockedId });

        modelBuilder.Entity<BlockedPlayer>()
            .HasOne(blockedPlayer => blockedPlayer.Player)
            .WithMany()
            .HasForeignKey(blockedPlayer => blockedPlayer.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BlockedPlayer>()
            .HasOne(blockedPlayer => blockedPlayer.Blocked)
            .WithMany()
            .HasForeignKey(blockedPlayer => blockedPlayer.BlockedId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<VerificationCode>()
            .HasOne(verificationCode => verificationCode.Player)
            .WithMany(player => player.VerificationCodes)
            .HasForeignKey(verificationCode => verificationCode.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Session>()
            .HasOne(session => session.Player)
            .WithMany(player => player.Sessions)
            .HasForeignKey(session => session.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatchResult>()
            .HasOne(matchResult => matchResult.Player)
            .WithMany(player => player.MatchResults)
            .HasForeignKey(matchResult => matchResult.PlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
