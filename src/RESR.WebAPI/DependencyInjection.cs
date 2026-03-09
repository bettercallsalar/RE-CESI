using FluentValidation;
using FluentValidation.AspNetCore;
using RESR.WebAPI.Routes.Users.Validators;

namespace RESR.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();
        services.AddEndpointsApiExplorer();

        return services;
    }
}
