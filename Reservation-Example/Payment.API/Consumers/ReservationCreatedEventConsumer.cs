using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Models;
using Payment.API.Models.Entities;
using Shared.Events;
using System.Text.Json;

namespace Payment.API.Consumers
{
    public class ReservationCreatedEventConsumer(PaymentApiDbContext contextDB) : IConsumer<ReservationCreatedEvent>
    {
        public async Task Consume(ConsumeContext<ReservationCreatedEvent> context)
        {
            bool result = await contextDB.PaymentInboxes.AnyAsync(i => i.IdempotentToken == context.Message.IdempotentToken);

            if (!result)
            {
                await contextDB.PaymentInboxes.AddAsync(new()
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
