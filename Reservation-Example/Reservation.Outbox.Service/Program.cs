using MassTransit;
using Quartz;
using Reservation.Outbox.Service.Jobs;

namespace Reservation.Outbox.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            // builder.Services.AddHostedService<Worker>();

            builder.Services.AddMassTransit(configurator =>
            {
                configurator.UsingRabbitMq((context, _configure) =>
                {
                    _configure.Host(builder.Configuration["RabbitMQ"]);
                });
            });

            builder.Services.AddQuartz(configurator =>
            {
                JobKey jobKey = new("ReservationOutboxPublishJob");
                configurator.AddJob<ReservationOutboxPublishJob>(options => options.WithIdentity(jobKey));

                TriggerKey triggerKey = new("ReservationOutboxPublishTrigger");
                configurator.AddTrigger(options => options.ForJob(jobKey)
                    .WithIdentity(triggerKey)
                    .StartAt(DateTime.UtcNow)
                    .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(5)
                    .RepeatForever())
                );
            });
            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            var host = builder.Build();
            host.Run();
        }
    }
}