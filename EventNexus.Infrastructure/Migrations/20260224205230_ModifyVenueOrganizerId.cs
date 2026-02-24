using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventNexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyVenueOrganizerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizerId",
                table: "Venues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OrganizerId",
                table: "Venues",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Organizers_OrganizerId",
                table: "Venues",
                column: "OrganizerId",
                principalTable: "Organizers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Organizers_OrganizerId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_OrganizerId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Venues");
        }
    }
}
