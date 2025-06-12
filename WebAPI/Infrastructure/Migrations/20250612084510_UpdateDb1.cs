using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CinemaRooms_Theater_TheaterId1",
                table: "CinemaRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Seat_CinemaRooms_CinemaRoomId",
                table: "Seat");

            migrationBuilder.DropTable(
                name: "Theater");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CinemaRooms",
                table: "CinemaRooms");

            migrationBuilder.DropIndex(
                name: "IX_CinemaRooms_TheaterId1",
                table: "CinemaRooms");

            migrationBuilder.DropColumn(
                name: "Col",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "SeatQuantity",
                table: "CinemaRooms");

            migrationBuilder.DropColumn(
                name: "TheaterId",
                table: "CinemaRooms");

            migrationBuilder.DropColumn(
                name: "TheaterId1",
                table: "CinemaRooms");

            migrationBuilder.RenameTable(
                name: "CinemaRooms",
                newName: "CinemaRoom");

            migrationBuilder.RenameColumn(
                name: "SeatName",
                table: "Seat",
                newName: "Label");

            migrationBuilder.AlterColumn<string>(
                name: "SeatType",
                table: "Seat",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ColIndex",
                table: "Seat",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CoupleGroupId",
                table: "Seat",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Seat",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Seat",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowIndex",
                table: "Seat",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CinemaRoom",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LayoutJson",
                table: "CinemaRoom",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalCols",
                table: "CinemaRoom",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRows",
                table: "CinemaRoom",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CinemaRoom",
                table: "CinemaRoom",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SeatTypePrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatType = table.Column<string>(type: "text", nullable: false),
                    DefaultPrice = table.Column<double>(type: "double precision", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatTypePrice", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatTypePrice_SeatType",
                table: "SeatTypePrice",
                column: "SeatType",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Seat_CinemaRoom_CinemaRoomId",
                table: "Seat",
                column: "CinemaRoomId",
                principalTable: "CinemaRoom",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seat_CinemaRoom_CinemaRoomId",
                table: "Seat");

            migrationBuilder.DropTable(
                name: "SeatTypePrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CinemaRoom",
                table: "CinemaRoom");

            migrationBuilder.DropColumn(
                name: "ColIndex",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "CoupleGroupId",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "RowIndex",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "LayoutJson",
                table: "CinemaRoom");

            migrationBuilder.DropColumn(
                name: "TotalCols",
                table: "CinemaRoom");

            migrationBuilder.DropColumn(
                name: "TotalRows",
                table: "CinemaRoom");

            migrationBuilder.RenameTable(
                name: "CinemaRoom",
                newName: "CinemaRooms");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "Seat",
                newName: "SeatName");

            migrationBuilder.AlterColumn<int>(
                name: "SeatType",
                table: "Seat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<char>(
                name: "Col",
                table: "Seat",
                type: "character(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "Seat",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CinemaRooms",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "SeatQuantity",
                table: "CinemaRooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TheaterId",
                table: "CinemaRooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TheaterId1",
                table: "CinemaRooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CinemaRooms",
                table: "CinemaRooms",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Theater",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theater", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CinemaRooms_TheaterId1",
                table: "CinemaRooms",
                column: "TheaterId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CinemaRooms_Theater_TheaterId1",
                table: "CinemaRooms",
                column: "TheaterId1",
                principalTable: "Theater",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Seat_CinemaRooms_CinemaRoomId",
                table: "Seat",
                column: "CinemaRoomId",
                principalTable: "CinemaRooms",
                principalColumn: "Id");
        }
    }
}
