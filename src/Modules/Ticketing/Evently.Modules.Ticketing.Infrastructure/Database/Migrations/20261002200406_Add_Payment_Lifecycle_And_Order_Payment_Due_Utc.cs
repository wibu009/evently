using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer 'static readonly' fields over constant array arguments

namespace Evently.Modules.Ticketing.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Add_Payment_Lifecycle_And_Order_Payment_Due_Utc : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<Guid>(
            name: "transaction_id",
            schema: "ticketing",
            table: "payments",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AlterColumn<string>(
            name: "currency",
            schema: "ticketing",
            table: "payments",
            type: "character varying(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AddColumn<string>(
            name: "failure_reason",
            schema: "ticketing",
            table: "payments",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "paid_at_utc",
            schema: "ticketing",
            table: "payments",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "status",
            schema: "ticketing",
            table: "payments",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<string>(
            name: "currency",
            schema: "ticketing",
            table: "orders",
            type: "character varying(3)",
            maxLength: 3,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AddColumn<string>(
            name: "cancellation_reason",
            schema: "ticketing",
            table: "orders",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "payment_due_utc",
            schema: "ticketing",
            table: "orders",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_orders_status_payment_due_utc",
            schema: "ticketing",
            table: "orders",
            columns: new[] { "status", "payment_due_utc" });

        // All payments created before the payment lifecycle existed were charged synchronously
        // during checkout, so they are backfilled as succeeded.
        migrationBuilder.Sql(
            """
            UPDATE ticketing.payments
            SET status = 1,
                paid_at_utc = created_at_utc
            WHERE status = 0;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_orders_status_payment_due_utc",
            schema: "ticketing",
            table: "orders");

        migrationBuilder.DropColumn(
            name: "payment_due_utc",
            schema: "ticketing",
            table: "orders");

        migrationBuilder.DropColumn(
            name: "cancellation_reason",
            schema: "ticketing",
            table: "orders");

        migrationBuilder.AlterColumn<string>(
            name: "currency",
            schema: "ticketing",
            table: "orders",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(3)",
            oldMaxLength: 3);

        migrationBuilder.DropColumn(
            name: "status",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.DropColumn(
            name: "paid_at_utc",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.DropColumn(
            name: "failure_reason",
            schema: "ticketing",
            table: "payments");

        migrationBuilder.AlterColumn<string>(
            name: "currency",
            schema: "ticketing",
            table: "payments",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(3)",
            oldMaxLength: 3);

        migrationBuilder.AlterColumn<Guid>(
            name: "transaction_id",
            schema: "ticketing",
            table: "payments",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);
    }
}
