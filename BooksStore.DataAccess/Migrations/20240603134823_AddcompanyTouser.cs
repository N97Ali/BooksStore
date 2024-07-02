using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddcompanyTouser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyId",
                table: "Companies",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Companies_CompanyId",
                table: "Companies",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Companies_CompanyId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_CompanyId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Companies");
        }
    }
}
