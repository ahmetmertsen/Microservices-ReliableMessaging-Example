using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class RabbitMQSettings
    {
        public const string Payment_ReservationCreatedEventQueue = "payment-reservation-created-event-queue";
        public const string Reservation_PaymentCompletedEventQueue = "reservation-payment-completed-event-queue";
        public const string Reservation_PaymentFailedEventQueue = "reservation-payment-failed-event-queue";
    }
}
