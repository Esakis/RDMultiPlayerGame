using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class ScienceTier45Effects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 17, 56, 52, 21, DateTimeKind.Utc).AddTicks(4915));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 25,
                column: "Description",
                value: "U Enta zmniejsza cenę budynków specjalnych o 21%. (Efekty oryginału — 3. rząd bez złota i tańsze przyspieszanie — nie mają odpowiednika: budynki specjalne kosztują tu wyłącznie budulec.)");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 26,
                column: "Description",
                value: "Budynki 6. i 7. rzędu budują się o turę szybciej (koszt budulca ×(t−1)/t). U Enta zmniejsza cenę budynków specjalnych o 30%.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 30,
                column: "Description",
                value: "Murarze zużywają 10% mniej kamienia. U Elfa zamiast tego rabat 32% na złoto zabudowań. Ent nie ma dostępu.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 31,
                column: "Description",
                value: "Wyburzanie budynków zwraca 80% budulca. U Ożywieńców zamiast tego rabat 32% na złoto zabudowań. Ent nie ma dostępu.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 3%.", 0.03m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 6%.", 0.06m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 20%.", 0.20m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 25%.", 0.25m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 45,
                column: "Description",
                value: "Efekt oryginału (kradzież zapasów z karawan) nie ma odpowiednika — karawany nie występują w tej wersji gry. Utrzymuje rabat złodziei 20%.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 46,
                column: "Description",
                value: "Efekt oryginału (ranni generałowie mają lvl/3) wymaga doprecyzowania źródła — odłożony. Utrzymuje rabat złodziei 20%.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 7, 10, 17, 45, 43, 16, DateTimeKind.Utc).AddTicks(6088));

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 25,
                column: "Description",
                value: "Budynki 3. rzędu nie kosztują złota niezależnie od czarnej magii; przyspieszanie budowy dodatkowo tańsze o 50%.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 26,
                column: "Description",
                value: "Budynki 6. i 7. rzędu budują się o turę szybciej.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 30,
                column: "Description",
                value: "Murarze zużywają 10% mniej kamienia.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 31,
                column: "Description",
                value: "Wyburzanie budynków zwraca 80% budulca.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 4,5%.", 0.045m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 9%.", 0.09m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 21%.", 0.21m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Description", "EffectValue" },
                values: new object[] { "Zmniejsza cenę czarów o 30%.", 0.30m });

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 45,
                column: "Description",
                value: "Umożliwia kradzież zapasów z karawan.");

            migrationBuilder.UpdateData(
                table: "TechnologyDefinitions",
                keyColumn: "Id",
                keyValue: 46,
                column: "Description",
                value: "Ranni generałowie zachowują poziom równy lvl/3.");
        }
    }
}
