using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketTaxHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerKingdomId = table.Column<int>(type: "int", nullable: false),
                    SellerKingdomId = table.Column<int>(type: "int", nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    PricePerUnit = table.Column<long>(type: "bigint", nullable: false),
                    GrossGold = table.Column<long>(type: "bigint", nullable: false),
                    Tax = table.Column<long>(type: "bigint", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketTransactions_Kingdoms_BuyerKingdomId",
                        column: x => x.BuyerKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketTransactions_Kingdoms_SellerKingdomId",
                        column: x => x.SellerKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 9, 20, 14, 893, DateTimeKind.Utc).AddTicks(96));

            migrationBuilder.CreateIndex(
                name: "IX_MarketTransactions_BuyerKingdomId",
                table: "MarketTransactions",
                column: "BuyerKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketTransactions_SellerKingdomId",
                table: "MarketTransactions",
                column: "SellerKingdomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketTransactions");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 9, 17, 14, 323, DateTimeKind.Utc).AddTicks(6149));
        }
    }
}
