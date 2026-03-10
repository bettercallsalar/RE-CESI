using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RESR.Core;
using RESR.Core.Security.Token;
using RESR.Infrastructure;
using RESR.WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiServices();

builder.Services.AddInfrastructure(builder.Configuration)
                .AddCoreServices(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
