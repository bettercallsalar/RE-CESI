using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESR.Core.Controllers.Departments.Ports;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RESR.Core.Controllers.Comments.Factories;
using RESR.Core.Controllers.Comments.Ports;
using RESR.Core.Controllers.Categories.Factories;
using RESR.Core.Controllers.Categories.Ports;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Permissions.Ports;
using RESR.Core.Controllers.Reactions.Factories;
using RESR.Core.Controllers.Reactions.Ports;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Controllers.Departments.Factories;
using RESR.Core.Controllers.Follows.Ports;
using RESR.Infrastructure.Departments;
using RESR.Infrastructure.Comments;
using RESR.Infrastructure.Categories;
using RESR.Infrastructure.Reactions;
using RESR.Infrastructure.Permissions;
using RESR.Infrastructure.Roles;
using RESR.Infrastructure.Users;
using RESR.Infrastructure.Follows;

namespace RESR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string DefaultConnection.");

        // Ensure category factory is available even if Core DI isn't wired as expected.
        services.TryAddScoped<ICategoryFactory, CategoryFactory>();
        services.TryAddScoped<ICommentFactory, CommentFactory>();
        services.TryAddScoped<IReactionFactory, ReactionFactory>();

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

        services.AddScoped<IDepartmentRepository>(sp =>
            new MySqlDepartmentRepository(
                connectionString,
                sp.GetRequiredService<IDepartmentFactory>()
            )
        );

        services.AddScoped<ICategoryRepository>(_ =>
            new MySqlCategoryRepository(
                connectionString,
                _.GetRequiredService<ICategoryFactory>()
            )
        );

        services.AddScoped<IFollowsRepository>(sp =>
            new MySqlFollowsRepository(
                connectionString
            )
        );

        services.AddScoped<ICommentRepository>(sp =>
            new MySqlCommentRepository(
                connectionString,
                sp.GetRequiredService<ICommentFactory>()
            )
        );

        services.AddScoped<IReactionRepository>(sp =>
            new MySqlReactionRepository(
                connectionString,
                sp.GetRequiredService<IReactionFactory>()
            )
        );

        return services;
    }
}
