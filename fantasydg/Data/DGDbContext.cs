using fantasydg.Models;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Data
{
    public class DGDbContext : DbContext
    {
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Player> Players { get; set; }

        public DGDbContext(DbContextOptions<DGDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite primary key for Tournament
            modelBuilder.Entity<Tournament>()
                .HasKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<Player>()
                .HasKey(p => new { p.Id, p.RoundId });

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Round)
                .WithMany(r => r.Players)
                .HasForeignKey(p => p.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Round>()
                .Property(r => r.Division)
                .IsRequired();
        }
    }
}