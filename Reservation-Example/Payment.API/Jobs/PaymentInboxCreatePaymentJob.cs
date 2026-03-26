using Microsoft.EntityFrameworkCore;
using Payment.API.Models;
using Quartz;
using Shared.Events;
using System.Text.Json;

namespace Payment.API.Jobs
{
    public class PaymentInboxCreatePaymentJob(PaymentApiDbContext contextDB) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var paymentInboxes = await contextDB.PaymentInboxes
                .Where(x => x.Processed == false)
                .ToListAsync();

            foreach (var inbox in paymentInboxes)
            {
                ReservationCreatedEvent reservationCreatedEvent = JsonSerializer.Deserialize<ReservationCreatedEvent>(inbox.Payload);

                Models.Entities.Payment payment = new()
                {
                    ReservationId = reservationCreatedEvent.ReservationId,
                    CreatedAt = DateTime.UtcNow,
                    PaymentStatus = Random.Shared.Next(6, 21) > 10 ? Models.Enums.PaymentStatus.Completed : Models.Enums.PaymentStatus.Failed
                };
                inbox.Processed = true;
                await contextDB.Payments.AddAsync(payment);
                await contextDB.SaveChangesAsync();
            }
        }
    }
}
