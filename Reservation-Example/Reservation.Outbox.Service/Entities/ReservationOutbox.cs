using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservation.Outbox.Service.Entities
{
    public class ReservationOutbox
    {
        public Guid IdempotentToken { get; set; }
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}

