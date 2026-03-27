using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class PaymentCompletedEvent
    {
        public Guid IdempotentToken { get; set; }
        public long ReservationId { get; set; }
    }
}
