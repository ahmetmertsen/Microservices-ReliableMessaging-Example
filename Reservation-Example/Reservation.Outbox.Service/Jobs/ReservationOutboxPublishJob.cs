using MassTransit;
using Quartz;
using Reservation.Outbox.Service.Entities;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Reservation.Outbox.Service.Jobs
{
    public class ReservationOutboxPublishJob(IPublishEndpoint publishEndpoint) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            if (ReservationOutboxSingletonDatabase.DataReaderState)
            {
                ReservationOutboxSingletonDatabase.DataReaderBusy();

                List<ReservationOutbox> reservationOutboxes =
                    (await ReservationOutboxSingletonDatabase.QueryAsync<ReservationOutbox>(
                        @"SELECT * FROM ""ReservationOutboxes"" WHERE ""ProcessedDate"" IS NULL ORDER BY ""OccurredOn"" ASC")).ToList();

                foreach (var reservationOutbox in reservationOutboxes)
                {
                    if (reservationOutbox.Type == nameof(ReservationCreatedEvent))
                    {
                        ReservationCreatedEvent reservationCreatedEvent = JsonSerializer.Deserialize<ReservationCreatedEvent>(reservationOutbox.Payload);
                        if (reservationCreatedEvent != null)
                        {
                            await publishEndpoint.Publish(reservationCreatedEvent);
                            await ReservationOutboxSingletonDatabase.ExecuteAsync(
                                @"UPDATE ""ReservationOutboxes"" SET ""ProcessedDate"" = NOW() WHERE ""IdempotentToken"" = @IdempotentToken",
                                    new { IdempotentToken = reservationOutbox.IdempotentToken }
                            );
                        }
                    }
                }
                ReservationOutboxSingletonDatabase.DataReaderReady();
                await Console.Out.WriteLineAsync("Reservation outbox table checked!");
            }
        }
    }
}
