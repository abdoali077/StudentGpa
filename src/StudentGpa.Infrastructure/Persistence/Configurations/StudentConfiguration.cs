using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.CreatedAtUtc).IsRequired();

        builder.HasIndex(s => s.Code).IsUnique();   // SQL Server default collation => case-insensitive uniqueness
    }
}
