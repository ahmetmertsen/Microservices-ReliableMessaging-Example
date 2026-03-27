namespace Payment.API.Models.Entities
{
    public class PaymentOutbox
    {
        public Guid IdempotentToken { get; set; }
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
