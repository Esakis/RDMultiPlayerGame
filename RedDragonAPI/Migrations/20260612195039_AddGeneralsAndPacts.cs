using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralsAndPacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Generals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KingdomId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimaryTrait = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecondaryTrait = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    IsOutside = table.Column<bool>(type: "bit", nullable: false),
                    IsImprisoned = table.Column<bool>(type: "bit", nullable: false),
                    WoundedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Generals_Kingdoms_KingdomId",
                        column: x => x.KingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposerKingdomId = table.Column<int>(type: "int", nullable: false),
                    TargetKingdomId = table.Column<int>(type: "int", nullable: false),
                    PactType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pacts_Kingdoms_ProposerKingdomId",
                        column: x => x.ProposerKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pacts_Kingdoms_TargetKingdomId",
                        column: x => x.TargetKingdomId,
                        principalTable: "Kingdoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 12, 19, 50, 38, 924, DateTimeKind.Utc).AddTicks(3851));

            migrationBuilder.CreateIndex(
                name: "IX_Generals_KingdomId",
                table: "Generals",
                column: "KingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacts_ProposerKingdomId",
                table: "Pacts",
                column: "ProposerKingdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Pacts_TargetKingdomId",
                table: "Pacts",
                column: "TargetKingdomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Generals");

            migrationBuilder.DropTable(
                name: "Pacts");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 12, 19, 27, 26, 988, DateTimeKind.Utc).AddTicks(2432));
        }
    }
}
