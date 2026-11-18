using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Infrastructure.Persistence.Configurations;

public sealed class GradeScaleConfiguration : IEntityTypeConfiguration<GradeScale>
{
    public void Configure(EntityTypeBuilder<GradeScale> builder)
    {
        builder.ToTable("GradeScales", t => t.HasCheckConstraint("CK_GradeScales_Range", "[FromGrade] < [ToGrade]"));
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();

        builder.Property(g => g.FromGrade).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(g => g.ToGrade).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(g => g.Points).HasColumnType("decimal(3,2)").IsRequired();
        builder.Property(g => g.Grade).IsRequired().HasMaxLength(5);

        builder.HasIndex(g => g.Grade).IsUnique();
        builder.HasIndex(g => g.FromGrade).IsUnique();

        // Scale lives in the database (seeded through the migration), not in the GPA code.
        builder.HasData(
            new GradeScale { Id = 1,  FromGrade = 96, ToGrade = 101, Points = 4.0m, Grade = "A+" },
            new GradeScale { Id = 2,  FromGrade = 92, ToGrade = 96,  Points = 3.7m, Grade = "A" },
            new GradeScale { Id = 3,  FromGrade = 88, ToGrade = 92,  Points = 3.4m, Grade = "A-" },
            new GradeScale { Id = 4,  FromGrade = 84, ToGrade = 88,  Points = 3.2m, Grade = "B+" },
            new GradeScale { Id = 5,  FromGrade = 80, ToGrade = 84,  Points = 3.0m, Grade = "B" },
            new GradeScale { Id = 6,  FromGrade = 76, ToGrade = 80,  Points = 2.8m, Grade = "B-" },
            new GradeScale { Id = 7,  FromGrade = 72, ToGrade = 76,  Points = 2.6m, Grade = "C+" },
            new GradeScale { Id = 8,  FromGrade = 68, ToGrade = 72,  Points = 2.4m, Grade = "C" },
            new GradeScale { Id = 9,  FromGrade = 64, ToGrade = 68,  Points = 2.2m, Grade = "C-" },
            new GradeScale { Id = 10, FromGrade = 60, ToGrade = 64,  Points = 2.0m, Grade = "D+" },
            new GradeScale { Id = 11, FromGrade = 55, ToGrade = 60,  Points = 1.5m, Grade = "D" },
            new GradeScale { Id = 12, FromGrade = 50, ToGrade = 55,  Points = 1.0m, Grade = "D-" },
            new GradeScale { Id = 13, FromGrade = 0,  ToGrade = 50,  Points = 0.0m, Grade = "F" });
    }
}
