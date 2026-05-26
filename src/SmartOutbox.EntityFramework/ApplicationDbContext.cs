using Microsoft.EntityFrameworkCore;
using SmartOutbox.Core.Entities;

namespace SmartOutbox.EntityFramework
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Amount)
                    .HasPrecision(18, 2);
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("outbox_messages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Payload)
                    .IsRequired()
                    .HasColumnType("text");
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                entity.Property(e => e.ProcessedAt);
                entity.Property(e => e.RetryCount)
                    .IsRequired()
                    .HasDefaultValue(0);
                entity.Property(e => e.Error)
                    .HasMaxLength(1000);
                entity.Property(e => e.NextAttemptAt)
                    .HasColumnType("timestamp with time zone");
                entity.HasIndex(e => new { e.ProcessedAt, e.RetryCount });
            });
        }
    }
}
