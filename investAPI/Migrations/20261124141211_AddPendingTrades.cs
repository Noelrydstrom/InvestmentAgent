using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace investAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingTrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SecurityId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Shares = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingTrades_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PendingTrades_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingTrades_SecurityId",
                table: "PendingTrades",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingTrades_UserId",
                table: "PendingTrades",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingTrades");
        }
    }
}
