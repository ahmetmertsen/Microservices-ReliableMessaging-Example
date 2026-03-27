using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reservation.API.Consumers;
using Reservation.API.Models;
using Reservation.API.Models.Dtos;
using Reservation.API.Models.Entities;
using Shared;
using Shared.Events;
using System.Text.Json;

namespace Reservation.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ReservationApiDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddMassTransit(configurator =>
            {
                configurator.AddConsumer<PaymentCompletedEventConsumer>();
                configurator.AddConsumer<PaymentFailedEventConsumer>();

                configurator.UsingRabbitMq((context, _configure) =>
                {
                    _configure.Host(builder.Configuration["RabbitMQ"]);

                    _configure.ReceiveEndpoint(RabbitMQSettings.Reservation_PaymentCompletedEventQueue, e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(context));
                    _configure.ReceiveEndpoint(RabbitMQSettings.Reservation_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapPost("/create-reservation", async (CreateReservationDto dto, ReservationApiDbContext context) =>
            {
                var eventEntity = await context.Events
                    .FirstOrDefaultAsync(e => e.Id == dto.EventId);

                if (eventEntity == null)
                {
                     return Results.NotFound(new { message = "Etkinlik bulunamadý." });
                }

                await using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    var reservation = new Models.Entities.Reservation
                    {
                        EventId = dto.EventId,
                        SeatNumber = dto.SeatNumber,
                        CustomerEmail = dto.CustomerEmail,
                        ReservationStatus = Models.Enums.ReservationStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Reservations.AddAsync(reservation);
                    await context.SaveChangesAsync();

                    var reservatedSeat = new ReservedSeat
                    {
                        ReservationId = reservation.Id,
                        EventId = reservation.EventId,
                        SeatNumber = reservation.SeatNumber
                    };
                    await context.ReservedSeats.AddAsync(reservatedSeat);

                    var idempotentToken = Guid.NewGuid();
                    ReservationCreatedEvent reservationCreatedEvent = new()
                    {
                        IdempotentToken = idempotentToken,
                        ReservationId = reservation.Id,
                        EventId = reservation.EventId,
                        SeatNumber = reservation.SeatNumber,
                        CustomerEmail = reservation.CustomerEmail,
                    };

                    ReservationOutbox reservationOutbox = new()
                    {
                        IdempotentToken = idempotentToken,
                        OccurredOn = DateTime.UtcNow,
                        ProcessedDate = null,
                        Payload = JsonSerializer.Serialize(reservationCreatedEvent),
                        Type = reservationCreatedEvent.GetType().Name
                    };
                    await context.ReservationOutboxes.AddAsync(reservationOutbox);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Results.Ok(new
                    {
                        message = "Rezervasyon oluþturuldu ve outbox'a yazýldý."
                    });
                } 
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    return Results.Conflict(new { message = "Bu koltuk zaten rezerve edilmiþ olabilir." });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return Results.Problem("Rezervasyon oluþturulurken bir hata oluþtu.");
                }
                
            });

            app.Run();
        }
    }
}
