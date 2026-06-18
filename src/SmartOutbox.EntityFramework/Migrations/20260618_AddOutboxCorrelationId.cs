using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOutbox.EntityFramework.Migrations
{
    public partial class AddOutboxCorrelationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_ProcessedAt_RetryCount",
                table: "outbox_messages");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "outbox_messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAt_RetryCount_NextAttemptAt",
                table: "outbox_messages",
                columns: new[] { "ProcessedAt", "RetryCount", "NextAttemptAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_ProcessedAt_RetryCount_NextAttemptAt",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "outbox_messages");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAt_RetryCount",
                table: "outbox_messages",
                columns: new[] { "ProcessedAt", "RetryCount" });
        }
    }
}
