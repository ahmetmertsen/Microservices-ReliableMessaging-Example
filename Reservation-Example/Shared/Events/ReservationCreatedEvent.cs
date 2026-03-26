using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class ReservationCreatedEvent
    {
        public Guid IdempotentToken { get; set; }
        public long ReservationId { get; set; }
        public long EventId { get; set; }
        public long SeatNumber { get; set; }
        public string CustomerEmail { get; set; }
    }
}
