using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EveryDaily.Application.Assembly;
using EveryDaily.Application.Extensions;
using EveryDaily.Application.Middleware;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Application.Socket;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

builder.Configuration.AddJsonFile($"appsettings.{env ?? ""}.json");
builder.Configuration.AddJsonFile($"JwtSettings.json");
builder.Configuration.AddJsonFile("emailSettings.json");

builder.Services.AddControllers();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailOptions"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
    {
        options.Tokens.PasswordResetTokenProvider = "passwordReset";
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.User.RequireUniqueEmail = false;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.EmailConfirmationTokenProvider = "TokenProvider";
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddDefaultTokenProviders()
    .AddUserStore<UserStore<UserEntity, RoleEntity, AppDbContext, Guid>>()
    .AddRoleStore<RoleStore<RoleEntity, AppDbContext, Guid>>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters.RoleClaimType = "role";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse(); // Varsayılan redirect'i engelle
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new { message = "Unauthorized" });
            }
        };
    });

builder.Services.AddTransient<ICacheService, CacheService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    string xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    setup.IncludeXmlComments(xmlPath);

    // Include 'SecurityScheme' to use JWT Authentication
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});


// Common
var dailyngoCors = "DailyngoCors";
builder.Services.ConfigureRedis(builder.Configuration);
builder.Services.ConfigureMassTransit(builder.Configuration);
builder.Services.ConfigureNpgsql(builder.Configuration);
builder.Services.ConfigureCors(dailyngoCors);
builder.Services.ConfigureMongoDbRepositories(builder.Configuration);
builder.Services.ConfigureServices();
builder.Services.AddSignalR();

#region JWT

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

builder.Services.AddScoped<JwtTokenGenerator>();

#endregion

builder.Services.AddMediatR(typeof(MediatorAssembly).Assembly);
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration s�ras�nda hata olu�tu: {ex.Message}");
        throw;
    }
}

try
{
    Seed.SeedCollectorData(app.Services).Wait();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

app.UseCors(dailyngoCors);

app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseResponseCaching();
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapHub<NotificationHub>("/notification-hub");

app.Run();