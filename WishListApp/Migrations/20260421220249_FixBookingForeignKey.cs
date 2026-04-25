using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WishListApp.Migrations
{
    /// <inheritdoc />
    public partial class FixBookingForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_BookedById",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookedById",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookedById",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookedByUserId",
                table: "Bookings",
                column: "BookedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_BookedByUserId",
                table: "Bookings",
                column: "BookedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_BookedByUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookedByUserId",
                table: "Bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "BookedById",
                table: "Bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookedById",
                table: "Bookings",
                column: "BookedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_BookedById",
                table: "Bookings",
                column: "BookedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
