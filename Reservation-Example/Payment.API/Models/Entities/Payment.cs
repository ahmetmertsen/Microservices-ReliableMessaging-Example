using Payment.API.Models.Enums;

namespace Payment.API.Models.Entities
{
    public class Payment
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
