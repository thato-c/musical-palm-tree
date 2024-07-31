using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCampus.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersiontoEnrolmentandCourseModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Enrolment",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Course",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Enrolment");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Course");
        }
    }
}
