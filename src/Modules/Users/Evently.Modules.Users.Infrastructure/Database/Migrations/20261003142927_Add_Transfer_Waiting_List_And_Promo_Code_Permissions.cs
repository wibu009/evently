using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Prefer 'static readonly' fields over constant array arguments

namespace Evently.Modules.Users.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Add_Transfer_Waiting_List_And_Promo_Code_Permissions : Migration
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
                    "promo-codes:create",
                    "promo-codes:read",
                    "promo-codes:remove",
                    "tickets:transfer",
                    "waiting-lists:join",
                    "waiting-lists:read",
                    "waiting-lists:remove"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "categories:read", "Member" },
                    { "events:read", "Member" },
                    { "promo-codes:create", "Administrator" },
                    { "promo-codes:read", "Administrator" },
                    { "promo-codes:remove", "Administrator" },
                    { "tickets:transfer", "Administrator" },
                    { "tickets:transfer", "Member" },
                    { "waiting-lists:join", "Administrator" },
                    { "waiting-lists:join", "Member" },
                    { "waiting-lists:read", "Administrator" },
                    { "waiting-lists:read", "Member" },
                    { "waiting-lists:remove", "Administrator" },
                    { "waiting-lists:remove", "Member" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "categories:read", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "events:read", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "promo-codes:create", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "promo-codes:read", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "promo-codes:remove", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "tickets:transfer", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "tickets:transfer", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "waiting-lists:join", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "waiting-lists:join", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "waiting-lists:read", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "waiting-lists:read", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "waiting-lists:remove", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "waiting-lists:remove", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "promo-codes:create");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "promo-codes:read");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "promo-codes:remove");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "tickets:transfer");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "waiting-lists:join");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "waiting-lists:read");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "waiting-lists:remove");
        }
}
