# Proje ) Reservation-Example

📌 Projenin Özellikleri

- Mimari: 4 servisli yapı (Reservation.API + Payment.API + Outbox.Services)

- Mesajlaşma: RabbitMQ (MassTransit ile event publish/consume)

- Teknolojiler: .NET 8, ASP.NET Core Minimal API, MassTransit, RabbitMQ, Entity Framework Core, PostgreSQL, Swagger, Docker (RabbitMQ)

Bu proje, mikroservislerde güvenilir mesajlaşma ve Outbox/Inbox pattern - Idempotent kullanımını pratik etmek amacıyla tasarlanmıştır.

## Akış Özet:
- Reservation.API
  - dış dünyaya REST endpoint sağlar
  - rezervasyon kaydını veritabanına ekler
  - ReservationCreatedEvent bilgisini ReservationOutbox tablosuna yazar
- Reservation.Outbox.Service
  - Quartz job ile belirli aralıklarla ReservationOutbox tablosunu kontrol eder
  - ProcessedDate = NULL olan kayıtları MassTransit üzerinden RabbitMQ’ya publish eder
  - başarılı publish sonrası outbox kaydını işlenmiş olarak günceller
- Payment.API
  - ReservationCreatedEvent mesajını consume eder
  - gelen mesajı önce PaymentInbox tablosuna kaydeder
  - ayrı Quartz job ile inbox tablosundaki işlenmemiş kayıtları okuyarak ödeme işlemini oluşturur
  - ödeme sonucuna göre: PaymentCompleted veya PaymentFailed üretir
  - bu event’leri doğrudan publish etmek yerine PaymentOutbox tablosuna yazar
- Payment.Outbox.Service
  - Quartz job ile PaymentOutbox tablosunu kontrol eder
  - işlenmemiş event’leri MassTransit üzerinden RabbitMQ’ya publish eder
  - publish başarılıysa ProcessedDate alanını günceller
- Reservation.API
  - PaymentCompletedEvent ve PaymentFailedEvent mesajlarını consume eder
  - mesajları önce ReservationInbox tablosuna kaydeder
  - Quartz job ile inbox kayıtlarını işleyerek rezervasyon durumunu günceller
