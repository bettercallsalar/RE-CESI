using RESR.Core.Users;
using RESR.Core.Users.Ports;
using RESR.Infrastructure.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Missing connection string DefaultConnection.");

builder.Services.AddScoped<IUserRepository>(_ => new MySqlUserRepository(cs));
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();
app.MapControllers();
app.Run();