using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentKart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentAndCategoryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RentalPricePerDay",
                table: "Equipment",
                newName: "RentalPrice");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Equipment",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Equipment",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Equipment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RentalPeriod",
                table: "Equipment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Equipment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_City",
                table: "Equipment",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Equipment_City",
                table: "Equipment");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "RentalPeriod",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Equipment");

            migrationBuilder.RenameColumn(
                name: "RentalPrice",
                table: "Equipment",
                newName: "RentalPricePerDay");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Equipment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
