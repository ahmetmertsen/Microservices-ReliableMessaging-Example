using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservation.API.Migrations
{
    /// <inheritdoc />
    public partial class mig_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservationInboxes",
                columns: table => new
                {
                    IdempotentToken = table.Column<Guid>(type: "uuid", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationInboxes", x => x.IdempotentToken);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationInboxes");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Reservations");
        }
    }
}
