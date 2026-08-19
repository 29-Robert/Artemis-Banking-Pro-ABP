using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArtemisBankingPro.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Cedula", "CommerceId", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "PhoneNumber", "RoleId", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, "00000000001", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@artemis.com", "Admin", true, null, "Defecto", "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2", null, 1, null, "admin" },
                    { 2, "00000000002", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cajero@artemis.com", "Cajero", true, null, "Defecto", "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2", null, 2, null, "cajero" },
                    { 3, "00000000003", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cliente@artemis.com", "Cliente", true, null, "Defecto", "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2", null, 3, null, "cliente" },
                    { 4, "00000000004", null, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "comercio@artemis.com", "Comercio", true, null, "Defecto", "$2a$11$mbZdNP5MPCF7U3YtCrhDI.RWLoQnKHSF0ryM91MJFw1ppT3aZTza2", null, 4, null, "comercio" }
                });

            migrationBuilder.InsertData(
                table: "SavingsAccounts",
                columns: new[] { "Id", "AccountNumber", "Balance", "BlockedAmount", "CreatedAt", "IsBlocked", "IsPrincipal", "Status", "Type", "UpdatedAt", "UserId" },
                values: new object[] { 1, "100200300", 5000.00m, 0m, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, true, 0, 0, null, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SavingsAccounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
