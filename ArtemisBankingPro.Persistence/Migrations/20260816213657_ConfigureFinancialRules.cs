using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBankingPro.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureFinancialRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsAccounts_Users_UserId",
                table: "SavingsAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SavingsAccounts_SavingsAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SavingsAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_SavingsAccounts_AccountNumber",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "SavingsAccountId",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "SavingsAccounts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_SavingsAccounts_AccountNumber",
                table: "SavingsAccounts",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountNumber",
                table: "Transactions",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsAccounts_AccountNumber",
                table: "SavingsAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsAccounts_Users_UserId",
                table: "SavingsAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SavingsAccounts_AccountNumber",
                table: "Transactions",
                column: "AccountNumber",
                principalTable: "SavingsAccounts",
                principalColumn: "AccountNumber",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsAccounts_Users_UserId",
                table: "SavingsAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SavingsAccounts_AccountNumber",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountNumber",
                table: "Transactions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SavingsAccounts_AccountNumber",
                table: "SavingsAccounts");

            migrationBuilder.DropIndex(
                name: "IX_SavingsAccounts_AccountNumber",
                table: "SavingsAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SavingsAccountId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "SavingsAccounts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SavingsAccountId",
                table: "Transactions",
                column: "SavingsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsAccounts_AccountNumber",
                table: "SavingsAccounts",
                column: "AccountNumber",
                unique: true,
                filter: "[AccountNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsAccounts_Users_UserId",
                table: "SavingsAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SavingsAccounts_SavingsAccountId",
                table: "Transactions",
                column: "SavingsAccountId",
                principalTable: "SavingsAccounts",
                principalColumn: "Id");
        }
    }
}
