using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeDefinitionLastUsedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUsedAt",
                table: "AttributeDefinitions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "AttributeDefinitions");
        }
    }
}
