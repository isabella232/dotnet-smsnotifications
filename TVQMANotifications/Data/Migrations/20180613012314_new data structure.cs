using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TVQMANotifications.Data.Migrations
{
    public partial class newdatastructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SendLogs_MessageId",
                table: "SendLogs");

            migrationBuilder.CreateIndex(
                name: "IX_SendLogs_MessageId",
                table: "SendLogs",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_SendLogs_SubscriberId",
                table: "SendLogs",
                column: "SubscriberId");

            migrationBuilder.AddForeignKey(
                name: "FK_SendLogs_Subscribers_SubscriberId",
                table: "SendLogs",
                column: "SubscriberId",
                principalTable: "Subscribers",
                principalColumn: "SubscriberId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SendLogs_Subscribers_SubscriberId",
                table: "SendLogs");

            migrationBuilder.DropIndex(
                name: "IX_SendLogs_MessageId",
                table: "SendLogs");

            migrationBuilder.DropIndex(
                name: "IX_SendLogs_SubscriberId",
                table: "SendLogs");

            migrationBuilder.CreateIndex(
                name: "IX_SendLogs_MessageId",
                table: "SendLogs",
                column: "MessageId",
                unique: true);
        }
    }
}
