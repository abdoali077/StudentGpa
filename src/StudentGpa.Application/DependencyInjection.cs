using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentGpa.Application.Interfaces;
using StudentGpa.Application.Services;

namespace StudentGpa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IGradeScaleService, GradeScaleService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IGpaService, GpaService>();

        return services;
    }
}
