using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBankingPro.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndAccountProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commerces_Users_UserId",
                table: "Commerces");

            migrationBuilder.DropForeignKey(
                name: "FK_SavingsAccounts_Users_ClientId",
                table: "SavingsAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Commerces_UserId",
                table: "Commerces");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Commerces");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "SavingsAccounts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SavingsAccounts_ClientId",
                table: "SavingsAccounts",
                newName: "IX_SavingsAccounts_UserId");

            migrationBuilder.AddColumn<int>(
                name: "CommerceId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrincipal",
                table: "SavingsAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CommerceId",
                table: "Users",
                column: "CommerceId",
                unique: true,
                filter: "[CommerceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsAccounts_Users_UserId",
                table: "SavingsAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Commerces_CommerceId",
                table: "Users",
                column: "CommerceId",
                principalTable: "Commerces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsAccounts_Users_UserId",
                table: "SavingsAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Commerces_CommerceId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CommerceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CommerceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPrincipal",
                table: "SavingsAccounts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SavingsAccounts",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_SavingsAccounts_UserId",
                table: "SavingsAccounts",
                newName: "IX_SavingsAccounts_ClientId");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Commerces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Commerces_UserId",
                table: "Commerces",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commerces_Users_UserId",
                table: "Commerces",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsAccounts_Users_ClientId",
                table: "SavingsAccounts",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
