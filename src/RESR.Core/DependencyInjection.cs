using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESR.Core.Controllers.Permissions;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Roles;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Departments;
using RESR.Core.Controllers.Departments.Factories;

namespace RESR.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IUserFactory, UserFactory>();
        services.AddScoped<IRoleFactory, RoleFactory>();
        services.AddScoped<IPermissionFactory, PermissionFactory>();
        services.AddScoped<IDepartmentFactory, DepartmentFactory>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IDepartmentService, DepartmentService>();

        return services;
    }
}
