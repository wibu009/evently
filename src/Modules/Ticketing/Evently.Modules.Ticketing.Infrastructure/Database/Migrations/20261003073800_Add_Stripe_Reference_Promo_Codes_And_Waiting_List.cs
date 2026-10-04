using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861 // Prefer 'static readonly' fields over constant array arguments

namespace Evently.Modules.Ticketing.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Add_Stripe_Reference_Promo_Codes_And_Waiting_List : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_payments_transaction_id",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.DropColumn(
            name: "transaction_id",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.AddColumn<string>(
            name: "transaction_reference",
            schema: "ticketing",
            table: "payments",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "discount_amount",
            schema: "ticketing",
            table: "orders",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<Guid>(
            name: "promo_code_id",
            schema: "ticketing",
            table: "orders",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "promo_codes",
            schema: "ticketing",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                discount_type = table.Column<int>(type: "integer", nullable: false),
                discount_value = table.Column<decimal>(type: "numeric", nullable: false),
                currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                max_redemptions = table.Column<int>(type: "integer", nullable: true),
                times_redeemed = table.Column<int>(type: "integer", nullable: false),
                valid_from_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                valid_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_promo_codes", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "waiting_list_entries",
            schema: "ticketing",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                ticket_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                notified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_waiting_list_entries", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_payments_transaction_reference",
            schema: "ticketing",
            table: "payments",
            column: "transaction_reference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_promo_codes_code",
            schema: "ticketing",
            table: "promo_codes",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_waiting_list_entries_ticket_type_id_customer_id",
            schema: "ticketing",
            table: "waiting_list_entries",
            columns: new[] { "ticket_type_id", "customer_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_waiting_list_entries_ticket_type_id_status",
            schema: "ticketing",
            table: "waiting_list_entries",
            columns: new[] { "ticket_type_id", "status" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "promo_codes",
            schema: "ticketing");

        migrationBuilder.DropTable(
            name: "waiting_list_entries",
            schema: "ticketing");

        migrationBuilder.DropIndex(
            name: "ix_payments_transaction_reference",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.DropColumn(
            name: "transaction_reference",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.DropColumn(
            name: "discount_amount",
            schema: "ticketing",
            table: "orders");

        migrationBuilder.DropColumn(
            name: "promo_code_id",
            schema: "ticketing",
            table: "orders");

        migrationBuilder.AddColumn<Guid>(
            name: "transaction_id",
            schema: "ticketing",
            table: "payments",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_payments_transaction_id",
            schema: "ticketing",
            table: "payments",
            column: "transaction_id",
            unique: true);
    }
}
