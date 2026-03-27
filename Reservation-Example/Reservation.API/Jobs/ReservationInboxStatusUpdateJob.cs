using Microsoft.EntityFrameworkCore;
using Quartz;
using Reservation.API.Models;
using Shared.Events;
using System.Text.Json;

namespace Reservation.API.Jobs
{
    public class ReservationInboxStatusUpdateJob(ReservationApiDbContext contextDB) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var reservationInboxes = await contextDB.ReservationInboxes
                .Where(x => x.Processed == false)
                .ToListAsync();

            foreach (var reservationInbox in reservationInboxes)
            { 
                if (reservationInbox.Type == nameof(PaymentCompletedEvent))
                {
                    PaymentCompletedEvent paymentCompletedEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(reservationInbox.Payload);
                    var reservation = await contextDB.Reservations
                        .FirstOrDefaultAsync(x => x.Id == paymentCompletedEvent.ReservationId);
                    
                    reservation.ReservationStatus = Models.Enums.ReservationStatus.Completed;
                    reservationInbox.Processed = true;
                }
                else if (reservationInbox.Type == nameof(PaymentFailedEvent))
                {
                    PaymentFailedEvent paymentFailedEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(reservationInbox.Payload);
                    var reservation = await contextDB.Reservations
                        .FirstOrDefaultAsync(x => x.Id == paymentFailedEvent.ReservationId);

                    reservation.ReservationStatus = Models.Enums.ReservationStatus.Failed;
                    reservation.Reason = paymentFailedEvent.Reason;
                    reservationInbox.Processed = true;
                }
            }
            await contextDB.SaveChangesAsync();
        }
    }
}
