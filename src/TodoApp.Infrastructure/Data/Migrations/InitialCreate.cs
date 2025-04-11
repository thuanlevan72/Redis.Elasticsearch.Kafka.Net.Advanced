using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Migration khởi tạo cơ sở dữ liệu ban đầu
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Thực hiện migration để tạo cơ sở dữ liệu
        /// </summary>
        /// <param name="migrationBuilder">Builder để tạo migration</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng Todos
            migrationBuilder.CreateTable(
                name: "Todos",
                columns: table => new
                {
                    // Tạo cột Id là khóa chính, kiểu Guid
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    
                    // Tạo cột Title kiểu string, tối đa 100 ký tự, không null
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    
                    // Tạo cột Description kiểu string, tối đa 500 ký tự, có thể null
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    
                    // Tạo cột IsCompleted kiểu boolean, không null
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    
                    // Tạo cột Priority kiểu int, không null
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    
                    // Tạo cột DueDate kiểu datetime, có thể null
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    
                    // Tạo cột CreatedAt kiểu datetime, không null
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    
                    // Tạo cột UpdatedAt kiểu datetime, có thể null
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    // Thiết lập khóa chính
                    table.PrimaryKey("PK_Todos", x => x.Id);
                });

            // Tạo index trên cột IsCompleted
            migrationBuilder.CreateIndex(
                name: "IX_Todos_IsCompleted",
                table: "Todos",
                column: "IsCompleted");

            // Tạo index trên cột Priority
            migrationBuilder.CreateIndex(
                name: "IX_Todos_Priority",
                table: "Todos",
                column: "Priority");

            // Tạo index trên cột DueDate
            migrationBuilder.CreateIndex(
                name: "IX_Todos_DueDate",
                table: "Todos",
                column: "DueDate");
        }

        /// <summary>
        /// Hoàn tác migration để xóa cơ sở dữ liệu
        /// </summary>
        /// <param name="migrationBuilder">Builder để hoàn tác migration</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa bảng Todos
            migrationBuilder.DropTable(
                name: "Todos");
        }
    }
}
