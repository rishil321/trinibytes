using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trinibytes.Migrations
{
    /// <inheritdoc />
    public partial class add_unique_job_id_constraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobEmploymentType",
                table: "CaribbeanJobsPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CaribbeanJobsPosts_CaribbeanJobsJobId",
                table: "CaribbeanJobsPosts",
                column: "CaribbeanJobsJobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CaribbeanJobsPosts_CaribbeanJobsJobId",
                table: "CaribbeanJobsPosts");

            migrationBuilder.DropColumn(
                name: "JobEmploymentType",
                table: "CaribbeanJobsPosts");
        }
    }
}
