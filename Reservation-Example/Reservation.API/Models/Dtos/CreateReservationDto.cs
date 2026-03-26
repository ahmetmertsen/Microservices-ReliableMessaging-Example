namespace Reservation.API.Models.Dtos
{
    public class CreateReservationDto
    {
        public long EventId { get; set; }
        public long SeatNumber { get; set; }
        public string CustomerEmail { get; set; }
    }
}
