using System.Text;
using CottageBookingSystem.API.Middleware;
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using CottageBookingSystem.Core.Security;
using CottageBookingSystem.Data;
using CottageBookingSystem.Data.Repositories;
using CottageBookingSystem.Service;
using CottageBookingSystem.Service.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args); // חייב להופיע בתחילת הקובץ

builder.Logging.ClearProviders();
builder.Host.UseNLog();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");
var jwtSettings = new JwtSettings(
    builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer was not configured."),
    builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience was not configured."),
    builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("JWT signing key was not configured."));
if (Encoding.UTF8.GetByteCount(jwtSettings.SigningKey) < 32)
{
    throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "UserId",
            RoleClaimType = "Role"
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy(RoleNames.Customer, policy => policy.RequireRole(RoleNames.Customer));
    options.AddPolicy(RoleNames.Admin, policy => policy.RequireRole(RoleNames.Admin));
});
builder.Services.AddAutoMapper(typeof(BookingMappingProfile).Assembly);
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICottageRepository, CottageRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICottageService, CottageService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// הגדרת CORS שמאפשרת פניות מהאתר שלך
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT access token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend"); // הפעלת ה-CORS

app.UseMiddleware<BookingConflictExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();