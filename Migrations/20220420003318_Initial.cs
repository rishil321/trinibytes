using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trinibytes.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaribbeanJobsPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaribbeanJobsJobId = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobCompany = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobSalary = table.Column<int>(type: "int", nullable: true),
                    JobMinEducationRequirement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullJobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobListingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobDeListingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobListingIsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaribbeanJobsPosts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaribbeanJobsPosts");
        }
    }
}
