using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudentGpa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreditHours = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.CheckConstraint("CK_Courses_CreditHours", "[CreditHours] BETWEEN 1 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "GradeScales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FromGrade = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ToGrade = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeScales", x => x.Id);
                    table.CheckConstraint("CK_GradeScales_Range", "[FromGrade] < [ToGrade]");
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                    table.CheckConstraint("CK_Semesters_DateRange", "[EndDate] > [StartDate]");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SemesterId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    CreditHours = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SpecialStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LetterGrade = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    GradePoints = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.CheckConstraint("CK_Enrollments_CreditHours", "[CreditHours] BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_Enrollments_Score", "[Score] IS NULL OR ([Score] >= 0 AND [Score] <= 100)");
                    table.CheckConstraint("CK_Enrollments_ScoreXorStatus", "([Score] IS NOT NULL AND [SpecialStatus] IS NULL) OR ([Score] IS NULL AND [SpecialStatus] IS NOT NULL)");
                    table.CheckConstraint("CK_Enrollments_ValidSpecialStatus", "[SpecialStatus] IS NULL OR [SpecialStatus] IN ('Absent', 'Withdrawn', 'Incomplete')");
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GradeScales",
                columns: new[] { "Id", "FromGrade", "Grade", "Points", "ToGrade" },
                values: new object[,]
                {
                    { 1, 96m, "A+", 4.0m, 101m },
                    { 2, 92m, "A", 3.7m, 96m },
                    { 3, 88m, "A-", 3.4m, 92m },
                    { 4, 84m, "B+", 3.2m, 88m },
                    { 5, 80m, "B", 3.0m, 84m },
                    { 6, 76m, "B-", 2.8m, 80m },
                    { 7, 72m, "C+", 2.6m, 76m },
                    { 8, 68m, "C", 2.4m, 72m },
                    { 9, 64m, "C-", 2.2m, 68m },
                    { 10, 60m, "D+", 2.0m, 64m },
                    { 11, 55m, "D", 1.5m, 60m },
                    { 12, 50m, "D-", 1.0m, 55m },
                    { 13, 0m, "F", 0.0m, 50m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_SemesterId",
                table: "Enrollments",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SemesterId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SemesterId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SemesterId_CourseId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SemesterId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeScales_FromGrade",
                table: "GradeScales",
                column: "FromGrade",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeScales_Grade",
                table: "GradeScales",
                column: "Grade",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_Code",
                table: "Semesters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_Code",
                table: "Students",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "GradeScales");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
