using MassTransit;
using Microsoft.EntityFrameworkCore;
using Reservation.API.Models;
using Shared.Events;
using System.Text.Json;

namespace Reservation.API.Consumers
{
    public class PaymentCompletedEventConsumer(ReservationApiDbContext contextDB) : IConsumer<PaymentCompletedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            bool result = await contextDB.ReservationInboxes.AnyAsync(i => i.IdempotentToken == context.Message.IdempotentToken);

            if (!result)
            {
                await contextDB.ReservationInboxes.AddAsync(new()
                {
                    IdempotentToken = context.Message.IdempotentToken,
                    Processed = false,
                    Payload = JsonSerializer.Serialize(context.Message)
                });
                await contextDB.SaveChangesAsync();
            }
        }
    }
}
