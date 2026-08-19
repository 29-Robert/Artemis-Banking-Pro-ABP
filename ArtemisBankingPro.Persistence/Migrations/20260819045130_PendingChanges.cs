using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBankingPro.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$1qnRUBRJHLZJfRzSwt2iuurx7EVZyXvctT5h1.wrMvUSlgV2CWBha");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$1qnRUBRJHLZJfRzSwt2iuurx7EVZyXvctT5h1.wrMvUSlgV2CWBha");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$1qnRUBRJHLZJfRzSwt2iuurx7EVZyXvctT5h1.wrMvUSlgV2CWBha");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$1qnRUBRJHLZJfRzSwt2iuurx7EVZyXvctT5h1.wrMvUSlgV2CWBha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$N9V2/U9qL5/7T.E8W5A29uT6l4G8B0rZ/Q/e1v0sD9t5R3N0Q9K8W");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$N9V2/U9qL5/7T.E8W5A29uT6l4G8B0rZ/Q/e1v0sD9t5R3N0Q9K8W");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$N9V2/U9qL5/7T.E8W5A29uT6l4G8B0rZ/Q/e1v0sD9t5R3N0Q9K8W");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$N9V2/U9qL5/7T.E8W5A29uT6l4G8B0rZ/Q/e1v0sD9t5R3N0Q9K8W");
        }
    }
}
