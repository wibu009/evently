using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861 // Prefer 'static readonly' fields over constant array arguments

namespace Evently.Modules.Events.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Add_Event_Visuals_And_Ticket_Design : Migration
{
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "background_image_url",
                schema: "events",
                table: "ticket_types",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                schema: "events",
                table: "ticket_types",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "accent_color",
                schema: "events",
                table: "events",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hero_banner_url",
                schema: "events",
                table: "events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "event_images",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_images_events_event_id",
                        column: x => x.event_id,
                        principalSchema: "events",
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_images_event_id",
                schema: "events",
                table: "event_images",
                column: "event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_images",
                schema: "events");

            migrationBuilder.DropColumn(
                name: "background_image_url",
                schema: "events",
                table: "ticket_types");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "events",
                table: "ticket_types");

            migrationBuilder.DropColumn(
                name: "accent_color",
                schema: "events",
                table: "events");

            migrationBuilder.DropColumn(
                name: "hero_banner_url",
                schema: "events",
                table: "events");
        }
}
