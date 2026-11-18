using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentGpa.Domain.Interfaces;
using StudentGpa.Domain.Services;
using StudentGpa.Infrastructure.Persistence;
using StudentGpa.Infrastructure.Persistence.Repositories;

namespace StudentGpa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure();
            }));

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IGradeScaleRepository, GradeScaleRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Stateless domain services (no I/O), registered here so the Domain project stays free of DI packages.
        services.AddSingleton<IAcademicPolicy, AcademicPolicy>();
        services.AddSingleton<IGradeResolver, GradeResolver>();
        services.AddSingleton<IEnrollmentGradingService, EnrollmentGradingService>();
        services.AddSingleton<IGpaCalculator, GpaCalculator>();

        return services;
    }
}
