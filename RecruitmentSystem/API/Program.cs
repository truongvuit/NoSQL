using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Core.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using API.Middleware;
using AutoMapper;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("https://localhost:7015", "http://localhost:5124");

            // Configuration
            builder.Services.Configure<DatabaseSettings>(
                builder.Configuration.GetSection("DatabaseSettings"));
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));
            builder.Services.Configure<RedisSettings>(
                builder.Configuration.GetSection("RedisSettings"));

            // MongoDB
            builder.Services.AddSingleton<MongoDbContext>();

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IJobRepository, JobRepository>();

            // Services
            builder.Services.AddSingleton<ICacheService, RedisCacheService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
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
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Controllers
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(Program));

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Avoid schema name collisions for types with same class name in different namespaces
                options.CustomSchemaIds(type => type.FullName);
            });
           
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDevelopmentAuth();
            }

            // Middleware Pipeline
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            // Development Auth Middleware (chỉ cho Development)
            if (app.Environment.IsDevelopment())
            {
                app.UseDevelopmentAuth();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}