namespace Reservation.API.Models.Entities
{
    public class ReservedSeat
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public long EventId { get; set; }
        public long SeatNumber { get; set; }

        public Reservation Reservation { get; set; }
        public Event Event { get; set; }
    }
}
