using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HangfireDemo.Migrations
{
    /// <inheritdoc />
    public partial class InitialWeather : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    Pressure = table.Column<double>(type: "float", nullable: true),
                    Humidity = table.Column<double>(type: "float", nullable: true),
                    CloudCover = table.Column<double>(type: "float", nullable: true),
                    WindSpeed = table.Column<double>(type: "float", nullable: true),
                    WindDirection = table.Column<double>(type: "float", nullable: true),
                    PrecipNextHour = table.Column<double>(type: "float", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherResults_Time",
                table: "WeatherResults",
                column: "Time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherResults");
        }
    }
}
