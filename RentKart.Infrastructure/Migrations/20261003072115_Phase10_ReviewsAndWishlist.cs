using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentKart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase10_ReviewsAndWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EquipmentId1",
                table: "WishlistItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EquipmentId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_EquipmentId1",
                table: "WishlistItems",
                column: "EquipmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId1",
                table: "Reviews",
                column: "BookingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BusinessId1",
                table: "Reviews",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_EquipmentId1",
                table: "Reviews",
                column: "EquipmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Bookings_BookingId1",
                table: "Reviews",
                column: "BookingId1",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Businesses_BusinessId1",
                table: "Reviews",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Equipment_EquipmentId1",
                table: "Reviews",
                column: "EquipmentId1",
                principalTable: "Equipment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Equipment_EquipmentId1",
                table: "WishlistItems",
                column: "EquipmentId1",
                principalTable: "Equipment",
                principalColumn: "Id");
        }
    }
}
