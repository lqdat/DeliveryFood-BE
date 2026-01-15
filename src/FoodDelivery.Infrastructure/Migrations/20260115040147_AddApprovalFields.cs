using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Restaurants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Merchants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicenseUrl",
                table: "Merchants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodSafetyCertUrl",
                table: "Merchants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdCardFrontUrl",
                table: "Merchants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Merchants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Merchants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseUrl",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdCardBackUrl",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdCardFrontUrl",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Drivers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleRegistrationUrl",
                table: "Drivers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "BusinessLicenseUrl",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "FoodSafetyCertUrl",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "IdCardFrontUrl",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Merchants");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "DriverLicenseUrl",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IdCardBackUrl",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IdCardFrontUrl",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "VehicleRegistrationUrl",
                table: "Drivers");
        }
    }
}
