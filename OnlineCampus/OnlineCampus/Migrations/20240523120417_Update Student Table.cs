using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineCampus.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Student",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Student",
                newName: "FirstName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Student",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Student",
                newName: "Description");
        }
    }
}
