using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliReserve.Migrations
{
    public partial class ownerInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {



            // Conversión explícita de uuid a integer usando SQL
            migrationBuilder.Sql("ALTER TABLE \"Businesses\" ALTER COLUMN \"OwnerId\" TYPE integer USING \"OwnerId\"::integer;");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_OwnerId",
                table: "Businesses",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Users_OwnerId",
                table: "Businesses",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {




            // Revertir el cambio de tipo de columna (si es necesario)
            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Businesses",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId1",
                table: "Businesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_OwnerId1",
                table: "Businesses",
                column: "OwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Users_OwnerId1",
                table: "Businesses",
                column: "OwnerId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
