using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.API.Consumers;
using Payment.API.Jobs;
using Payment.API.Models;
using Quartz;
using Shared;

namespace Payment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<PaymentApiDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddMassTransit(configurator =>
            {
                configurator.AddConsumer<ReservationCreatedEventConsumer>();

                configurator.UsingRabbitMq((context, _configure) =>
                {
                    _configure.Host(builder.Configuration["RabbitMQ"]);

                    _configure.ReceiveEndpoint(RabbitMQSettings.Payment_ReservationCreatedEventQueue, e => e.ConfigureConsumer<ReservationCreatedEventConsumer>(context));
                });
            });

            builder.Services.AddQuartz(configurator =>
            {
                JobKey jobKey = new("PaymentInboxCreatePaymentJob");
                configurator.AddJob<PaymentInboxCreatePaymentJob>(options => options.WithIdentity(jobKey));

                TriggerKey triggerKey = new("PaymentInboxCreatePaymentTrigger");
                configurator.AddTrigger(options => options.ForJob(jobKey)
                    .WithIdentity(triggerKey)
                    .StartAt(DateTime.UtcNow)
                    .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(5)
                    .RepeatForever())
                );
            });
            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            var app = builder.Build();


            app.Run();
        }
    }
}
