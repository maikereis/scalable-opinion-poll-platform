using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parrhesia.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Surveys",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Surveys", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Questions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SurveyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Questions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Questions_Surveys_SurveyId",
                    column: x => x.SurveyId,
                    principalTable: "Surveys",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Options",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SurveyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Options", x => x.Id);
                table.ForeignKey(
                    name: "FK_Options_Surveys_SurveyId",
                    column: x => x.SurveyId,
                    principalTable: "Surveys",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Ballots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SurveyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SelectedOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VoterFingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                DeviceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IpHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                CastedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ballots", x => x.Id);
            });

        // Indexes
        migrationBuilder.CreateIndex(
            name: "IX_Surveys_Status",
            table: "Surveys",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Questions_SurveyId",
            table: "Questions",
            column: "SurveyId");

        migrationBuilder.CreateIndex(
            name: "IX_Options_SurveyId",
            table: "Options",
            column: "SurveyId");

        migrationBuilder.CreateIndex(
            name: "IX_Options_QuestionId",
            table: "Options",
            column: "QuestionId");

        migrationBuilder.CreateIndex(
            name: "IX_Ballots_SurveyId",
            table: "Ballots",
            column: "SurveyId");

        migrationBuilder.CreateIndex(
            name: "IX_Ballots_SelectedOptionId",
            table: "Ballots",
            column: "SelectedOptionId");

        migrationBuilder.CreateIndex(
            name: "IX_Ballots_CastedAt",
            table: "Ballots",
            column: "CastedAt");

        // Unique constraint: one vote per user per survey
        migrationBuilder.CreateIndex(
            name: "IX_Ballots_VoterFingerprint_SurveyId",
            table: "Ballots",
            columns: new[] { "VoterFingerprint", "SurveyId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Ballots");
        migrationBuilder.DropTable(name: "Options");
        migrationBuilder.DropTable(name: "Questions");
        migrationBuilder.DropTable(name: "Surveys");
    }
}