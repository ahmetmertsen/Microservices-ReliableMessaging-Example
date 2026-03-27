using MassTransit;
using Payment.Outbox.Service.Entities;
using Quartz;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payment.Outbox.Service.Jobs
{
    public class PaymentOutboxPublishJob(IPublishEndpoint publishEndpoint) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (PaymentOutboxSingletonDatabase.DataReaderState)
            {
                PaymentOutboxSingletonDatabase.DataReaderBusy();

                List<PaymentOutbox> paymentOutboxes =
                    (await PaymentOutboxSingletonDatabase.QueryAsync<PaymentOutbox>(
                        @"SELECT * FROM ""PaymentOutboxes"" WHERE ""ProcessedDate"" IS NULL ORDER BY ""OccurredOn"" ASC")).ToList();

                foreach (var paymentOutbox in paymentOutboxes)
                {
                    if (paymentOutbox.Type == nameof(PaymentCompletedEvent))
                    {
                        PaymentCompletedEvent paymentCompletedEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(paymentOutbox.Payload);
                        if (paymentCompletedEvent != null)
                        {
                            await publishEndpoint.Publish(paymentCompletedEvent);
                            await PaymentOutboxSingletonDatabase.ExecuteAsync(
                                @"UPDATE ""PaymentOutboxes"" SET ""ProcessedDate"" = NOW() WHERE ""IdempotentToken"" = @IdempotentToken",
                                    new { IdempotentToken = paymentOutbox.IdempotentToken }
                            );
                        }
                    }
                    else if (paymentOutbox.Type == nameof(PaymentFailedEvent))
                    {
                        PaymentFailedEvent paymentFailedEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(paymentOutbox.Payload);
                        if (paymentFailedEvent != null)
                        {
                            await publishEndpoint.Publish(paymentFailedEvent);
                            await PaymentOutboxSingletonDatabase.ExecuteAsync(
                                @"UPDATE ""PaymentOutboxes"" SET ""ProcessedDate"" = NOW() WHERE ""IdempotentToken"" = @IdempotentToken",
                                    new { IdempotentToken = paymentOutbox.IdempotentToken }
                            );
                        }
                    }
                }
                PaymentOutboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Payment outbox table checked!");
            }
        }
    }
}
