using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Seatsure.Application.Interfaces;
using Seatsure.Application.Notifications;
using Seatsure.Application.Security;
using Seatsure.Application.Services.Imp;
using Seatsure.Application.Services.Interfaces;
using Seatsure.Infrastructure.Data;
using Seatsure.Infrastructure.Repositories;
using Seatsure.Middleware;
using Seatsure.Hubs;
using Seatsure.Notifications;

namespace Seatsure;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.Configure<Jwtoptions>(builder.Configuration.GetSection(Jwtoptions.sectionName));
        var jwt = builder.Configuration.GetSection(Jwtoptions.sectionName).Get<Jwtoptions>() ?? new Jwtoptions();
        if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes.");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                    ValidateAudience = true, ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.Email,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });
        builder.Services.AddAuthorization();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
        builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        builder.Services.AddScoped<IJWTService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<ITicketTypeService, TicektTypeService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<IAvailabilityNotification, SignalRAvailabilityNotification>();
        builder.Services.AddHostedService<HoldExpiryService>();

        builder.Services.AddSignalR();
        builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
                In = ParameterLocation.Header, Description = "Enter: Bearer {token}"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
            });
        });

        var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
        app.UseHttpsRedirection();
        app.UseStatusCodePages(async statusContext =>
        {
            var response = statusContext.HttpContext.Response;
            if (response.StatusCode is not 401 and not 403) return;
            response.ContentType = "application/problem+json";
            await response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = response.StatusCode,
                Title = response.StatusCode == 401 ? "Unauthorized" : "Forbidden",
                Detail = response.StatusCode == 401 ? "Authentication is required." : "You are not allowed to perform this action."
            });
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<AvailabilityHub>("/hubs/events");
        app.Run();
    }
}
