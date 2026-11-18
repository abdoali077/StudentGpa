using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Infrastructure.Persistence.Configurations;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", t =>
        {
            t.HasCheckConstraint("CK_Enrollments_Score", "[Score] IS NULL OR ([Score] >= 0 AND [Score] <= 100)");
            t.HasCheckConstraint("CK_Enrollments_CreditHours", "[CreditHours] BETWEEN 1 AND 3");
            t.HasCheckConstraint("CK_Enrollments_ValidSpecialStatus",
                "[SpecialStatus] IS NULL OR [SpecialStatus] IN ('Absent', 'Withdrawn', 'Incomplete')");
            // A registration has EITHER a numeric score OR a special status - never both, never neither.
            t.HasCheckConstraint("CK_Enrollments_ScoreXorStatus",
                "([Score] IS NOT NULL AND [SpecialStatus] IS NULL) OR ([Score] IS NULL AND [SpecialStatus] IS NOT NULL)");
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreditHours).IsRequired();
        builder.Property(e => e.Score).HasColumnType("decimal(5,2)");
        builder.Property(e => e.SpecialStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.LetterGrade).IsRequired().HasMaxLength(5);
        builder.Property(e => e.GradePoints).HasColumnType("decimal(3,2)");
        builder.Property(e => e.CreatedAtUtc).IsRequired();

        builder.Ignore(e => e.IsGpaEligible);
        builder.Ignore(e => e.QualityPoints);
        builder.Ignore(e => e.DisplayGrade);

        builder.HasOne(e => e.Student).WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Semester).WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SemesterId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course).WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);

        // Same course cannot be registered twice for the same student in the same semester.
        builder.HasIndex(e => new { e.StudentId, e.SemesterId, e.CourseId }).IsUnique();
        builder.HasIndex(e => new { e.StudentId, e.SemesterId });
        builder.HasIndex(e => e.CourseId);
    }
}


