using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentGpa.Domain.Constants;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", t => t.HasCheckConstraint(
            "CK_Courses_CreditHours",
            $"[CreditHours] BETWEEN {AcademicRules.MinCourseCreditHours} AND {AcademicRules.MaxCourseCreditHours}"));

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CreditHours).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        builder.HasIndex(c => c.Code).IsUnique();
    }
}
