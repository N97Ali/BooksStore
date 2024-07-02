using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddcolPaymentDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueDate",
                table: "OrderHeaders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDueDate",
                table: "OrderHeaders");
        }
    }
}
