using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Infrastructure.Persistence.Configurations;

public sealed class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("Semesters", t => t.HasCheckConstraint("CK_Semesters_DateRange", "[EndDate] > [StartDate]"));
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.StartDate).HasColumnType("date").IsRequired();
        builder.Property(s => s.EndDate).HasColumnType("date").IsRequired();
        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.HasIndex(s => s.Code).IsUnique();
    }
}
