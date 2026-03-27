using Microsoft.EntityFrameworkCore;
using Payment.API.Models;
using Payment.API.Models.Entities;
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
                #region Inbox
                ReservationCreatedEvent reservationCreatedEvent = JsonSerializer.Deserialize<ReservationCreatedEvent>(inbox.Payload);

                bool paymentOk = Random.Shared.Next(6, 21) > 10;
                Models.Entities.Payment payment = new()
                {
                    ReservationId = reservationCreatedEvent.ReservationId,
                    CreatedAt = DateTime.UtcNow,
                    PaymentStatus = paymentOk ? Models.Enums.PaymentStatus.Completed : Models.Enums.PaymentStatus.Failed
                };
                inbox.Processed = true;
                await contextDB.Payments.AddAsync(payment);

                #endregion

                #region Outbox
                object integrationEvent;

                if (paymentOk)
                {
                    integrationEvent = new PaymentCompletedEvent
                    {
                        IdempotentToken = reservationCreatedEvent.IdempotentToken,
                        ReservationId = payment.ReservationId,
                    };
                }
                else
                {
                    integrationEvent = new PaymentFailedEvent
                    {
                        IdempotentToken = reservationCreatedEvent.IdempotentToken,
                        ReservationId = payment.ReservationId,
                        Reason = "Ödeme başarısız oldu."
                    };
                }

                PaymentOutbox paymentOutbox = new()
                {
                    IdempotentToken = reservationCreatedEvent.IdempotentToken,
                    OccurredOn = DateTime.UtcNow,
                    ProcessedDate = null,
                    Type = integrationEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(integrationEvent)
                };
                await contextDB.PaymentOutboxes.AddAsync(paymentOutbox);

                #endregion

                await contextDB.SaveChangesAsync();
            }
        }
    }
}
