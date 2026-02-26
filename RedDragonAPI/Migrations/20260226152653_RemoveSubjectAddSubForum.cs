using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedDragonAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubjectAddSubForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "ForumPosts");

            migrationBuilder.AddColumn<string>(
                name: "SubForum",
                table: "ForumPosts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 2, 26, 15, 26, 52, 626, DateTimeKind.Utc).AddTicks(3307));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubForum",
                table: "ForumPosts");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "ForumPosts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Eras",
                keyColumn: "Id",
                keyValue: 1,
                column: "StartedAt",
                value: new DateTime(2026, 2, 26, 15, 15, 55, 975, DateTimeKind.Utc).AddTicks(5096));
        }
    }
}
