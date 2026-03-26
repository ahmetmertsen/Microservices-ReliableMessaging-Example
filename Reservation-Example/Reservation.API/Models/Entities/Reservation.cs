using Reservation.API.Models.Enums;

namespace Reservation.API.Models.Entities
{
    public class Reservation
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long SeatNumber { get; set; }
        public string CustomerEmail { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        public Event Event { get; set; }
    }
}
