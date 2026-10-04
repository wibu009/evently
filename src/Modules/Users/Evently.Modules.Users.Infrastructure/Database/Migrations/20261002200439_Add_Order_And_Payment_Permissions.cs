using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer 'static readonly' fields over constant array arguments

namespace Evently.Modules.Users.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Add_Order_And_Payment_Permissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            schema: "users",
            table: "permissions",
            column: "code",
            values: new object[]
            {
                "orders:cancel",
                "payments:read",
                "payments:refund"
            });

        migrationBuilder.InsertData(
            schema: "users",
            table: "role_permissions",
            columns: new[] { "permission_code", "role_name" },
            values: new object[,]
            {
                { "orders:cancel", "Administrator" },
                { "orders:cancel", "Member" },
                { "payments:read", "Administrator" },
                { "payments:read", "Member" },
                { "payments:refund", "Administrator" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "orders:cancel", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "orders:cancel", "Member" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "payments:read", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "payments:read", "Member" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "role_permissions",
            keyColumns: new[] { "permission_code", "role_name" },
            keyValues: new object[] { "payments:refund", "Administrator" });

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "orders:cancel");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "payments:read");

        migrationBuilder.DeleteData(
            schema: "users",
            table: "permissions",
            keyColumn: "code",
            keyValue: "payments:refund");
    }
}
