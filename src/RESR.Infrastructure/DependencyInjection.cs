using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Permissions.Ports;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Infrastructure.Permissions;
using RESR.Infrastructure.Roles;
using RESR.Infrastructure.Users;

namespace RESR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string DefaultConnection.");

        services.AddScoped<IUserRepository>(sp =>
            new MySqlUserRepository(
                connectionString,
                sp.GetRequiredService<IUserFactory>()
            )
        );

        services.AddScoped<IRoleRepository>(sp =>
            new MySqlRoleRepository(
                connectionString,
                sp.GetRequiredService<IRoleFactory>(),
                sp.GetRequiredService<IPermissionFactory>()
            )
        );

        services.AddScoped<IPermissionRepository>(sp =>
            new MySqlPermissionRepository(
                connectionString,
                sp.GetRequiredService<IPermissionFactory>()
            )
        );

        return services;
    }
}
