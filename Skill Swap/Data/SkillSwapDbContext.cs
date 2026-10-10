using Microsoft.EntityFrameworkCore;
using SkillSwap.Models;
using SkillSwap.Models.Enums;

namespace SkillSwap.Data
{

    public class SkillSwapDbContext : DbContext
    {

        public SkillSwapDbContext()
        {
        }

        public SkillSwapDbContext(DbContextOptions<SkillSwapDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=.;Database=SkillSwapConsoleDb;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<SkillOffer> SkillOffers => Set<SkillOffer>();
        public DbSet<SkillRequest> SkillRequests => Set<SkillRequest>();
        public DbSet<SwapRequest> SwapRequests => Set<SwapRequest>();
        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
            });

            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100)
                      .IsUnicode();

                entity.HasIndex(e => e.Name)
                      .IsUnique();
            });

            modelBuilder.Entity<SkillOffer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Offers)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Skill)
                      .WithMany(s => s.Offers)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.SkillId })
                      .IsUnique();
            });

            modelBuilder.Entity<SkillRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Requests)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Skill)
                      .WithMany(s => s.Requests)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.SkillId })
                      .IsUnique();
            });

            modelBuilder.Entity<SwapRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Sender)
                      .WithMany(u => u.SentSwaps)
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Receiver)
                      .WithMany(u => u.ReceivedSwaps)
                      .HasForeignKey(e => e.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OfferedSkill)
                      .WithMany(s => s.OfferedInSwaps)
                      .HasForeignKey(e => e.OfferedSkillId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RequestedSkill)
                      .WithMany(s => s.RequestedInSwaps)
                      .HasForeignKey(e => e.RequestedSkillId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status)
                      .HasDefaultValue(SwapStatus.Pending);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.SwapRequest)
                      .WithMany(s => s.Sessions)
                      .HasForeignKey(e => e.SwapRequestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}