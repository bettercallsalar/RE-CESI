using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RESR.Core.Controllers.Comments;
using RESR.Core.Controllers.Comments.Factories;
using RESR.Core.Controllers.Categories;
using RESR.Core.Controllers.Categories.Factories;
using RESR.Core.Controllers.Permissions;
using RESR.Core.Controllers.Permissions.Factories;
using RESR.Core.Controllers.Reactions;
using RESR.Core.Controllers.Reactions.Factories;
using RESR.Core.Controllers.Roles;
using RESR.Core.Controllers.Roles.Factories;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Departments;
using RESR.Core.Controllers.Departments.Factories;
using RESR.Core.Controllers.Articles;
using RESR.Core.Controllers.Articles.Factories;
using RESR.Core.Controllers.Events;
using RESR.Core.Controllers.Events.Factories;
using RESR.Core.Controllers.Follows;
using RESR.Core.Controllers.Marks;

namespace RESR.Core;

public static class DependencyInjection
{
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
                services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

                services.AddScoped<IPasswordHasher, PasswordHasher>();
                services.AddScoped<ITokenService, TokenService>();

                services.AddScoped<ICategoryFactory, CategoryFactory>();
                services.AddScoped<ICommentFactory, CommentFactory>();
                services.AddScoped<IReactionFactory, ReactionFactory>();
                services.AddScoped<IUserFactory, UserFactory>();
                services.AddScoped<IRoleFactory, RoleFactory>();
                services.AddScoped<IPermissionFactory, PermissionFactory>();
                services.AddScoped<IDepartmentFactory, DepartmentFactory>();
                services.AddScoped<IArticleFactory, ArticleFactory>();
                services.AddScoped<IEventFactory, EventFactory>();

                services.AddScoped<ICommentService, CommentService>();
                services.AddScoped<IReactionService, ReactionService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IRoleService, RoleService>();
                services.AddScoped<IPermissionService, PermissionService>();
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<IDepartmentService, DepartmentService>();
                services.AddScoped<IArticleService, ArticleService>();
                services.AddScoped<IEventService, EventService>();
                services.AddScoped<IFollowsService, FollowsService>();
                services.AddScoped<IMarkService, MarkService>();

                return services;
        }
}
