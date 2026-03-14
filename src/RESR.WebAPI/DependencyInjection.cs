using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using RESR.WebAPI.Routes.Users.Validators;

namespace RESR.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var firstError = context.ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage)
                        .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));

                    return new BadRequestObjectResult(new
                    {
                        message = string.IsNullOrWhiteSpace(firstError)
                            ? "The request payload is invalid."
                            : firstError
                    });
                };
            });
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();
        services.AddEndpointsApiExplorer();

        return services;
    }
}
