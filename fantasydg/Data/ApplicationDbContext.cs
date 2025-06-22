﻿using fantasydg.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<PlayerTournament> PlayerTournaments { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamPlayer> TeamPlayers { get; set; }
        public DbSet<LeagueTournament> LeagueTournaments { get; set; }
        public DbSet<TeamPlayerTournament> TeamPlayerTournaments { get; set; }
        public DbSet<LeagueMember> LeagueMembers { get; set; }
        public DbSet<LeaguePlayerFantasyPoints> LeaguePlayerFantasyPoints { get; set; }
        public DbSet<LeagueInvitation> LeagueInvitations { get; set; }
        public DbSet<LeagueOwnershipTransfer> LeagueOwnershipTransfers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //

            modelBuilder.Entity<Player>()
                .HasKey(p => p.PDGANumber);

            // Tournament Relationships
            modelBuilder.Entity<Tournament>()
                .HasKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<LeagueTournament>()
                .Property(lt => lt.Weight)
                .HasDefaultValue(1);

            modelBuilder.Entity<LeagueTournament>()
                .HasKey(lt => new { lt.LeagueId, lt.TournamentId, lt.Division });

            modelBuilder.Entity<LeagueTournament>()
                .HasOne(lt => lt.League)
                .WithMany(l => l.LeagueTournaments)
                .HasForeignKey(lt => lt.LeagueId);

            modelBuilder.Entity<LeagueTournament>()
                .HasOne(lt => lt.Tournament)
                .WithMany(t => t.LeagueTournaments)
                .HasForeignKey(lt => new { lt.TournamentId, lt.Division })
                .HasPrincipalKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<PlayerTournament>()
                .HasOne(pt => pt.Tournament)
                .WithMany(t => t.PlayerTournaments)
                .HasForeignKey(pt => new { pt.TournamentId, pt.Division })
                .HasPrincipalKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<TeamPlayerTournament>()
                .HasKey(tpt => new { tpt.TeamId, tpt.PDGANumber, tpt.TournamentId, tpt.Division });

            modelBuilder.Entity<TeamPlayerTournament>()
                .HasOne(tpt => tpt.Team)
                .WithMany(t => t.TeamPlayerTournaments)
                .HasForeignKey(tpt => tpt.TeamId);

            modelBuilder.Entity<TeamPlayerTournament>()
                .HasOne(tpt => tpt.Player)
                .WithMany(p => p.TeamPlayerTournaments)
                .HasForeignKey(tpt => tpt.PDGANumber);

            modelBuilder.Entity<TeamPlayerTournament>()
                .HasOne(tpt => tpt.Tournament)
                .WithMany()
                .HasForeignKey(tpt => new { tpt.TournamentId, tpt.Division });

            // Identity-based League System
            modelBuilder.Entity<LeagueMember>()
                .HasKey(lm => new { lm.LeagueId, lm.UserId });

            modelBuilder.Entity<League>()
                .HasMany(l => l.Members)
                .WithOne(m => m.League)
                .HasForeignKey(m => m.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<League>()
                .HasMany(l => l.Teams)
                .WithOne(t => t.League)
                .HasForeignKey(t => t.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasKey(tp => new { tp.TeamId, tp.PDGANumber });

            modelBuilder.Entity<TeamPlayer>()
                .HasOne(tp => tp.Team)
                .WithMany(t => t.TeamPlayers)
                .HasForeignKey(tp => tp.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamPlayer>()
                .HasOne(tp => tp.Player)
                .WithMany(p => p.TeamPlayers)
                .HasForeignKey(tp => tp.PDGANumber)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamPlayer>()
                .HasIndex(tp => new { tp.TeamId, tp.PDGANumber })
                .IsUnique();

            modelBuilder.Entity<LeaguePlayerFantasyPoints>()
                .HasKey(lp => new { lp.LeagueId, lp.PDGANumber, lp.TournamentId });

            modelBuilder.Entity<LeaguePlayerFantasyPoints>()
                .HasOne(lp => lp.League)
                .WithMany()
                .HasForeignKey(lp => lp.LeagueId);

            modelBuilder.Entity<LeaguePlayerFantasyPoints>()
                .HasOne(lp => lp.Player)
                .WithMany()
                .HasForeignKey(lp => lp.PDGANumber);

            modelBuilder.Entity<LeaguePlayerFantasyPoints>()
                .HasOne(lp => lp.Tournament)
                .WithMany()
                .HasForeignKey(lp => new { lp.TournamentId, lp.Division });

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