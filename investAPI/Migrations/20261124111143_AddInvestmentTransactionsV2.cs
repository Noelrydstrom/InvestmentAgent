using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace investAPI.Migrations
{
    public partial class AddInvestmentTransactionsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add cash balance to existing users
            migrationBuilder.AddColumn<decimal>(
            name: "CashBalance",
            table: "Users",
            type: "decimal(12,2)",
            nullable: false,
            defaultValue: 0m);

            // 2. Change shares from int to decimal
            migrationBuilder.AlterColumn<decimal>(
                name: "Shares",
                table: "Holdings",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // 3. Create investment transactions
            migrationBuilder.CreateTable(
                name: "InvestmentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    UserId = table.Column<int>(type: "int", nullable: false),

                    SecurityId = table.Column<int>(type: "int", nullable: false),

                    TransactionType = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false),

                    Shares = table.Column<decimal>(
                        type: "decimal(18,6)",
                        nullable: false),

                    Price = table.Column<decimal>(
                        type: "decimal(12,2)",
                        nullable: false),

                    TotalAmount = table.Column<decimal>(
                        type: "decimal(12,2)",
                        nullable: false),

                    TransactionDate = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_InvestmentTransactions",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_InvestmentTransactions_Securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "Securities",
                        principalColumn: "Id");

                    table.ForeignKey(
                        name: "FK_InvestmentTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTransactions_SecurityId",
                table: "InvestmentTransactions",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTransactions_UserId",
                table: "InvestmentTransactions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentTransactions");

            migrationBuilder.DropColumn(
                name: "CashBalance",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "Shares",
                table: "Holdings",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");
        }
    }

}
