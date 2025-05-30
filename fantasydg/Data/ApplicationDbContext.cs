﻿using fantasydg.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<PlayerTournament> PlayerTournaments { get; set; }
        public DbSet<RoundScore> RoundScores { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamPlayer> TeamPlayers { get; set; }
        public DbSet<LeagueMember> LeagueMembers { get; set; }
        public DbSet<LeagueInvitation> LeagueInvitations { get; set; }
        public DbSet<LeagueOwnershipTransfer> LeagueOwnershipTransfers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //

            // Tournament Relationships
            modelBuilder.Entity<Tournament>()
                .HasKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<PlayerTournament>()
                .HasKey(pt => new { pt.PlayerId, pt.TournamentId, pt.Division });

            modelBuilder.Entity<PlayerTournament>()
                .HasOne(pt => pt.Tournament)
                .WithMany(t => t.PlayerTournaments)
                .HasForeignKey(pt => new { pt.TournamentId, pt.Division })
                .HasPrincipalKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<Round>()
                .HasKey(r => r.RoundId);

            modelBuilder.Entity<Round>()
                .HasOne(r => r.Tournament)
                .WithMany(t => t.Rounds)
                .HasForeignKey(r => new { r.TournamentId, r.Division })
                .HasPrincipalKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<RoundScore>()
                .HasKey(rs => new { rs.RoundId, rs.PlayerId });

            modelBuilder.Entity<RoundScore>()
                .HasOne(rs => rs.Round)
                .WithMany(r => r.RoundScores)
                .HasForeignKey(rs => rs.RoundId);

            // Identity-based League System
            modelBuilder.Entity<LeagueMember>()
                .HasKey(lm => new { lm.LeagueId, lm.UserId });

            modelBuilder.Entity<League>()
                .HasOne(l => l.Owner)
                .WithMany(u => u.LeaguesOwned)
                .HasForeignKey(l => l.OwnerId);

            modelBuilder.Entity<LeagueMember>()
                .HasOne(lm => lm.League)
                .WithMany(l => l.Members)
                .HasForeignKey(lm => lm.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeagueMember>()
                .HasOne(lm => lm.User)
                .WithMany(u => u.LeagueMemberships)
                .HasForeignKey(lm => lm.UserId);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.TeamsOwned)
                .HasForeignKey(t => t.OwnerId);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.League)
                .WithMany(l => l.Teams)
                .HasForeignKey(t => t.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasIndex(t => new { t.LeagueId, t.OwnerId }) // One team per user per league
                .IsUnique();

            // TeamPlayer Many-to-Many with enforcement: One player per league
            modelBuilder.Entity<TeamPlayer>()
                .HasKey(tp => new { tp.TeamId, tp.PlayerId });

            modelBuilder.Entity<TeamPlayer>()
                .HasOne(tp => tp.Team)
                .WithMany(t => t.TeamPlayers)
                .HasForeignKey(tp => tp.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamPlayer>()
                .HasOne(tp => tp.Player)
                .WithMany(p => p.TeamPlayers)
                .HasForeignKey(tp => tp.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamPlayer>()
                .HasIndex(tp => new { tp.LeagueId, tp.PlayerId })
                .IsUnique();

            modelBuilder.Entity<LeagueInvitation>()
                .HasOne(i => i.League)
                .WithMany()
                .HasForeignKey(i => i.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeagueOwnershipTransfer>()
                .HasOne(l => l.League)
                .WithMany()
                .HasForeignKey(l => l.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}