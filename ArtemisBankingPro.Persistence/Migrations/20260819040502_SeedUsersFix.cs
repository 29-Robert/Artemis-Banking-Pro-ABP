using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBankingPro.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsersFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$raOHSgJdzeW1Npb/XugJlOJGGQb9zq.UE42.RJaB3rANcCtV0j1g.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$raOHSgJdzeW1Npb/XugJlOJGGQb9zq.UE42.RJaB3rANcCtV0j1g.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$raOHSgJdzeW1Npb/XugJlOJGGQb9zq.UE42.RJaB3rANcCtV0j1g.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$raOHSgJdzeW1Npb/XugJlOJGGQb9zq.UE42.RJaB3rANcCtV0j1g.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2");
        }
    }
}
