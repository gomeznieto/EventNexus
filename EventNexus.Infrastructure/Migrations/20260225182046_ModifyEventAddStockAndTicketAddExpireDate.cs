using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyEventAddStockAndTicketAddExpireDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchasedDate",
                table: "Tickets",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "BussinessPhone",
                table: "Organizers",
                newName: "BusinessPhone");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableTickets",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AvailableTickets",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "Tickets",
                newName: "PurchasedDate");

            migrationBuilder.RenameColumn(
                name: "BusinessPhone",
                table: "Organizers",
                newName: "BussinessPhone");
        }
    }
}
