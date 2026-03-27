using Microsoft.EntityFrameworkCore;
using Payment.API.Models.Entities;

namespace Payment.API.Models
{
    public class PaymentApiDbContext : DbContext
    {
        public PaymentApiDbContext(DbContextOptions<PaymentApiDbContext> options) : base(options) { }

        public DbSet<Entities.Payment> Payments { get; set; }
        public DbSet<PaymentInbox> PaymentInboxes { get; set; }
        public DbSet<PaymentOutbox> PaymentOutboxes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Payment>()
                .Property(p => p.PaymentStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Entities.Payment>()
                .HasIndex(p => p.ReservationId)
                .IsUnique();

            modelBuilder.Entity<PaymentInbox>()
                .HasKey(i => i.IdempotentToken);

            modelBuilder.Entity<PaymentOutbox>()
                .HasKey(o => o.IdempotentToken);

            base.OnModelCreating(modelBuilder);
        }
    }
}
