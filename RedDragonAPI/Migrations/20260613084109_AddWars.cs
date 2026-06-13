using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EraId = table.Column<int>(type: "int", nullable: false),
                    DeclaringCoalitionId = table.Column<int>(type: "int", nullable: false),
                    TargetCoalitionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeclaredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wars_Coalitions_DeclaringCoalitionId",
                        column: x => x.DeclaringCoalitionId,
                        principalTable: "Coalitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wars_Coalitions_TargetCoalitionId",
                        column: x => x.TargetCoalitionId,
                        principalTable: "Coalitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wars_Eras_EraId",
                        column: x => x.EraId,
                        principalTable: "Eras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 8, 41, 8, 656, DateTimeKind.Utc).AddTicks(3831));

            migrationBuilder.CreateIndex(
                name: "IX_Wars_DeclaringCoalitionId",
                table: "Wars",
                column: "DeclaringCoalitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_EraId_Status",
                table: "Wars",
                columns: new[] { "EraId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Wars_TargetCoalitionId",
                table: "Wars",
                column: "TargetCoalitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wars");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 6, 13, 8, 15, 34, 711, DateTimeKind.Utc).AddTicks(7993));
        }
    }
}
