namespace Payment.API.Models.Entities
{
    public class PaymentInbox
    {
        public Guid IdempotentToken { get; set; }
        public bool Processed { get; set; }
        public string Payload { get; set; }
    }
}
