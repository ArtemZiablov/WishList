using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WishListApp.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibilityAndAccessRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteToken",
                table: "WishLists",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "WishLists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WishListAccessRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WishListId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishListAccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishListAccessRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishListAccessRequests_WishLists_WishListId",
                        column: x => x.WishListId,
                        principalTable: "WishLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishListAccessRequests_RequestedByUserId",
                table: "WishListAccessRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WishListAccessRequests_WishListId_RequestedByUserId",
                table: "WishListAccessRequests",
                columns: new[] { "WishListId", "RequestedByUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WishListAccessRequests");

            migrationBuilder.DropColumn(
                name: "InviteToken",
                table: "WishLists");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "WishLists");
        }
    }
}
