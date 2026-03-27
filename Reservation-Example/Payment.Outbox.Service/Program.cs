using MassTransit;
using Payment.Outbox.Service.Jobs;
using Quartz;

namespace Payment.Outbox.Service
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
                JobKey jobKey = new("PaymentOutboxPublishJob");
                configurator.AddJob<PaymentOutboxPublishJob>(options => options.WithIdentity(jobKey));

                TriggerKey triggerKey = new("PaymentOutboxPublishTrigger");
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