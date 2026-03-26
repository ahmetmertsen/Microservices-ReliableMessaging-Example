namespace Reservation.API.Models.Entities
{
    public class Event
    {
        public long Id { get; set; }
        public long AvailableSeats { get; set; }
        public string Price { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
    }
}
