using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RESR.Core;
using RESR.Core.Errors;
using RESR.Core.Controllers.Resources.Ports;
using RESR.Core.Security.Token;
using RESR.Infrastructure;
using RESR.WebAPI;
using RESR.WebAPI.Files;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddWebApiServices();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
    });
});

builder.Services.AddInfrastructure(builder.Configuration)
                .AddCoreServices(builder.Configuration);
builder.Services.AddScoped<IResourceFileStorage, LocalResourceFileStorage>();

var jwtSecret = builder.Configuration[$"{JwtSettings.SectionName}:SecretKey"]
    ?? throw new InvalidOperationException("Missing JwtSettings:SecretKey.");
var jwtIssuer = builder.Configuration[$"{JwtSettings.SectionName}:Issuer"]
    ?? throw new InvalidOperationException("Missing JwtSettings:Issuer.");
var jwtAudience = builder.Configuration[$"{JwtSettings.SectionName}:Audience"]
    ?? throw new InvalidOperationException("Missing JwtSettings:Audience.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            RequireExpirationTime = false,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESR API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc, null),
            new List<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
    catch (ConflictException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
    catch (NotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
});

app.UseCors("Frontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
