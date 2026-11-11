using CottageBookingSystem.API;
using CottageBookingSystem.API.Middleware;
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using CottageBookingSystem.Core.Security;
using CottageBookingSystem.Data;
using CottageBookingSystem.Data.PostgreSql;
using CottageBookingSystem.Data.Repositories;
using CottageBookingSystem.Service;
using CottageBookingSystem.Service.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// הגדרת חיבור למסד הנתונים (SQL Server LocalDB)
// בחירת סוג מסד הנתונים לפי ההגדרות
var databaseProvider = builder.Configuration["DatabaseProvider"];

if (databaseProvider == "PostgreSql")
{
    builder.Services.AddDbContext<PostgresDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PostgresConnection"),
        npgsql => npgsql.MigrationsHistoryTable(
            "__EFMigrationsHistory_PostgreSql")));
    builder.Services.AddScoped<AppDbContext>(services =>
        services.GetRequiredService<PostgresDbContext>());
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}
// הגדרת CORS לאפשר תקשורת חלקה עם צד הלקוח (Frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// טעינת הגדרות JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSettings = new JwtSettings(
    jwtSection["Issuer"] ?? string.Empty,
    jwtSection["Audience"] ?? string.Empty,
    jwtSection["SigningKey"] ?? string.Empty
);
builder.Services.AddSingleton(jwtSettings);

// הגדרת אימות JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        RoleClaimType = "Role",
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
    };
});

// רישום שירותים ו־Repositories ב־DI (Dependency Injection)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICottageRepository, CottageRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICottageService, CottageService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddControllers();

// הגדרת Swagger ותמיכה בטוקנים של JWT בממשק התיעוד
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CottageBookingSystem.API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "הכנס טוקן JWT עם הרווח לפניו בצורה הבאה: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
var app = builder.Build();


// הגדרת ה־Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<BookingConflictExceptionMiddleware>();
app.UseHttpsRedirection();

// הפעלת מדיניות ה־CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("STARTUP: AdminSeeder starting");
await AdminSeeder.SeedAsync(app.Services, app.Configuration);
Console.WriteLine("STARTUP: AdminSeeder completed");

Console.WriteLine("STARTUP: DemoDataSeeder starting");
await DemoDataSeeder.SeedAsync(app.Services, app.Configuration);
Console.WriteLine("STARTUP: DemoDataSeeder completed");

Console.WriteLine("STARTUP: Starting HTTP server");
app.Run();