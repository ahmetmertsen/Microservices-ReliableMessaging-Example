namespace Reservation.API.Models.Entities
{
    public class ReservationInbox
    {
        public Guid IdempotentToken { get; set; }
        public bool Processed { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
