using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtemisBankingPro.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommerceIdToCreditCardConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommerceId",
                table: "CreditCardConsumptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CreditCardConsumptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardConsumptions_CommerceId",
                table: "CreditCardConsumptions",
                column: "CommerceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditCardConsumptions_Commerces_CommerceId",
                table: "CreditCardConsumptions",
                column: "CommerceId",
                principalTable: "Commerces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditCardConsumptions_Commerces_CommerceId",
                table: "CreditCardConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_CreditCardConsumptions_CommerceId",
                table: "CreditCardConsumptions");

            migrationBuilder.DropColumn(
                name: "CommerceId",
                table: "CreditCardConsumptions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CreditCardConsumptions");
        }
    }
}
