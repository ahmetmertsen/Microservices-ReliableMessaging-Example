using Microsoft.EntityFrameworkCore;
using Reservation.API.Models.Entities;

namespace Reservation.API.Models
{
    public class ReservationApiDbContext : DbContext
    {
        public ReservationApiDbContext(DbContextOptions<ReservationApiDbContext> options) : base(options) { }

        public DbSet<Entities.Reservation> Reservations { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<ReservedSeat> ReservedSeats { get; set; }
        public DbSet<ReservationOutbox> ReservationOutboxes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Reservation>()
                .Property(r => r.ReservationStatus)
                .HasConversion<string>();

            modelBuilder.Entity<ReservedSeat>()
                .HasIndex(rs => new { rs.EventId, rs.SeatNumber })
                .IsUnique();

            modelBuilder.Entity<ReservationOutbox>()
                .HasKey(r => r.IdempotentToken);
                

            base.OnModelCreating(modelBuilder);
        }
    }
}
