using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESR.Core.Controllers.Categories;
using RESR.Core.Controllers.Categories.Factories;
using RESR.Core.Controllers.Permissions;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Roles;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;

namespace RESR.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<ICategoryFactory, CategoryFactory>();
        services.AddScoped<IUserFactory, UserFactory>();
        services.AddScoped<IRoleFactory, RoleFactory>();
        services.AddScoped<IPermissionFactory, PermissionFactory>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
