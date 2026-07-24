using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeValueSelectedOptionFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AttributeValues_SelectedOptionId",
                table: "AttributeValues",
                column: "SelectedOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeValues_AttributeOptions_SelectedOptionId",
                table: "AttributeValues",
                column: "SelectedOptionId",
                principalTable: "AttributeOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeValues_AttributeOptions_SelectedOptionId",
                table: "AttributeValues");

            migrationBuilder.DropIndex(
                name: "IX_AttributeValues_SelectedOptionId",
                table: "AttributeValues");
        }
    }
}
