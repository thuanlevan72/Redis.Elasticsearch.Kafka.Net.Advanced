using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Application.Common.Behaviors;

namespace TodoApp.Application;

/// <summary>
/// Lớp mở rộng để đăng ký các dependency của tầng Application
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các dịch vụ của tầng Application vào container
    /// </summary>
    /// <param name="services">Container dịch vụ</param>
    /// <returns>Container dịch vụ đã được cấu hình</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký tự động tất cả các handler của MediatR trong assembly hiện tại
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Đăng ký tự động tất cả các validator của FluentValidation trong assembly hiện tại
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Đăng ký behavior validation cho request pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Đăng ký AutoMapper với các profile trong assembly hiện tại
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
